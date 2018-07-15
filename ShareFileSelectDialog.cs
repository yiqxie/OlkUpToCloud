using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Resources;
using System.Reflection;
//using AttachmentToWos.CloudWebService;
using System.Security.Cryptography;
using System.Diagnostics;

namespace FileToUpload
{
    public partial class ShareFileSelectDialog : Form
    {
        private WosHttpClient httpClient;// = new WosHttpClient("http://10.20.19.202");
        private String strToken = String.Empty;
        private String strRepoID = String.Empty;
        private String strCurrentFolder = "/";
        private List<String> lstPreviousfolder = new List<String>();
        public String strSelectedFile = String.Empty;

        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

        public ShareFileSelectDialog()
        {
            InitializeComponent();
        }

        /// 进行DES加密。
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串。</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        private string Encrypt(string pToEncrypt, string sKey)
        {
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                    des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    string str = Convert.ToBase64String(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return String.Empty;
            }
        }

        /// <summary>
        /// 进行DES解密。
        /// </summary>
        /// <param name="pToDecrypt">要解密的以Base64</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>已解密的字符串。</returns>
        private string Decrypt(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    string str = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return String.Empty;
            }
        }        

        private String SeafileLogon()
        {
            Boolean bFoundToken = false;

            String strDesKey = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Substring(0,8);
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            try
            {
                if (regKey != null)
                {
                    strToken = Decrypt(regKey.GetValue("token").ToString(),strDesKey);
                    if (strToken != null && strToken != String.Empty)
                    {
                        Boolean b = httpClient.ExecuteSeaFilePingRequest(strToken);

                        if (b)
                        {
                            LogTrace.TraceVerbose("Retrieve Token {0} from register.", strToken);
                            return strToken;
                        }
                    }
                }
                else
                {
                    LogTrace.TraceError("Access to Registry key {0} failed.", @"Software\Microsoft\Office\14.0\Outlook\Addins\FileToUpload");
                    bFoundToken = false;
                }
            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                bFoundToken = false;
            }

            // Retrieve Token for current user
            if (!bFoundToken)
            {
                String strLoginUser, strLoginPassword;
                LogonDialog dlg = new LogonDialog();

                DialogResult result = dlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    strLoginUser = dlg.EmailAddress;
                    strLoginPassword = dlg.Password;

                    LogTrace.TraceVerbose("Retrieve token with user: {0}, password: {1}",strLoginUser,"******");
                }
                else
                {
                    LogTrace.TraceInfo("Cancel logon.");
                    return null;
                }

                HttpSeaFileTokenResponse rsp = httpClient.ExecuteSeaFileTokenRequest(strLoginUser, strLoginPassword);
                if (rsp != null)
                {
                    regKey.SetValue("token", Encrypt(rsp.Token,strDesKey));
                    strToken = rsp.Token;
                    LogTrace.TraceVerbose("Retrieve Token {0} succeed.",strToken);
                    return strToken;
                }
                else
                {
                    MessageBox.Show("1、如果您已是云盘用户，请输入正确的邮箱密码（开机密码）\n\r2、如果您还不是云盘用户请联系所在机构IT咨询如何申请开通，不要重复尝试登录避免邮箱帐号被锁定", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogTrace.TraceError("Retrieve token failed.");
                    //sdlg.ShowDialog();
                    return null;
                }
            }
            return null;
        }

        private void ListDirectory(HttpSeaFileGetDirectoryEntriesResponse gdResp)
        {
            if (gdResp == null)
            {
                LogTrace.TraceError("The entries ar empty.");
                return;
            }

            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(48, 48);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            imgList.Images.Add("Folder", Properties.Resources.folder);
            imgList.Images.Add("File", Properties.Resources.file);

            lvFilelist.LargeImageList = imgList;

            //Add Return folder
            if (strCurrentFolder != "/")
            {
                LogTrace.TraceInfo("Current folder [{0}] is not root folder, add back folder.",strCurrentFolder);

                ListViewItem lvItem = new ListViewItem();
                lvItem.ImageIndex = 0;
                lvItem.Text = "上一层目录...";
                lvItem.Tag = "dirback";
                lvItem.ToolTipText = "";

                lvFilelist.Items.Add(lvItem);
            }

            List<HttpSeaFileGetDirectoryEntriesNode> lstDirectry = gdResp.GetDirectoryEntries();

            foreach (HttpSeaFileGetDirectoryEntriesNode node in lstDirectry)
            {
                ListViewItem lvItem = new ListViewItem();
                lvItem.ImageIndex = 0;
                lvItem.Text = node.Name;
                lvItem.Tag = node.Type;
                lvItem.ToolTipText = node.ID;

                lvFilelist.Items.Add(lvItem);

                LogTrace.TraceVerbose("Add folder [{0}:{1}:{2}] into list.", node.Name, node.Type, node.ID);
            }

            List<HttpSeaFileGetDirectoryEntriesNode> lstEntry = gdResp.GetFileEntries();

            foreach (HttpSeaFileGetDirectoryEntriesNode node in lstEntry)
            {
                ListViewItem lvi = new ListViewItem();
                //lvi.ImageIndex = 1;
                lvi.Text = node.Name;
                lvi.Tag = node.Type;
                lvi.ToolTipText = node.ID;

                int iExtStart = node.Name.LastIndexOf('.');
                iExtStart = iExtStart < 0 ? 0 : iExtStart;
                String strExt = node.Name.Substring(iExtStart);

                try
                {
                    if (lvFilelist.LargeImageList.Images[strExt] == null)
                    {
                        LogTrace.TraceVerbose("Add Icon for extention {0} into image list.", strExt);
                        lvFilelist.LargeImageList.Images.Add(strExt, Icons.IconFromExtension(strExt, Icons.SystemIconSize.Large));
                    }

                    lvi.ImageKey = strExt;
                    LogTrace.TraceVerbose("Add {0} Icon for file {1}",strExt,node.Name);
                }
                catch(Exception ept)
                {
                    LogTrace.TraceInfo("Ignore this exception.");
                    LogTrace.TraceException(ept);
                    lvi.ImageKey = "file";
                }

                lvFilelist.Items.Add(lvi);

                LogTrace.TraceVerbose("Add file [{0}:{1}:{2}] into list.", node.Name, node.Type, node.ID);
            }
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvFilelist.SelectedItems)
            {
                LogTrace.TraceVerbose("Selected Item [{0}:{1}:{2}, Current folder [{3}], Selected file [{4}]", item.Text, item.Tag, item.Index, strCurrentFolder, strSelectedFile);
                
                if (item.Tag.ToString() != "dirback")
                {
                    if (strSelectedFile == null || strSelectedFile == String.Empty || strSelectedFile == "")
                    {
                        strSelectedFile = strCurrentFolder + "/" + item.Text;

                        LogTrace.TraceVerbose("Selected file or folder {0}", strSelectedFile);
                    }
                    else
                    {
                        strSelectedFile = strSelectedFile + ";" + strCurrentFolder + "/" + item.Text;
                        LogTrace.TraceVerbose("Selected file or folder {0}", strSelectedFile);
                    }
                }
            }

            this.Close();
        }

        private void ShareFileSelectDialog_Load(object sender, EventArgs e)
        {
            lstPreviousfolder.Add("/");

            string strUrl = Globals.ThisAddIn.GetSeafileURL();

            if (strUrl != null && strUrl != "" && strUrl != String.Empty)
            {
                httpClient = new WosHttpClient(strUrl);
                LogTrace.TraceVerbose("Connect to fileshare with URL {0}",strUrl);
            }
            else
            {
                LogTrace.TraceError("Got fileshare URL failed.");
                this.Close();
                return;
            }

            strToken = SeafileLogon();

            if (strToken == null)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            HttpSeaFileGetDefaultLibraryResponse gdlRsp = httpClient.ExecuteSeaFileGetDefaultLibraryRequest(strToken);

            if (gdlRsp != null)
            {
                strRepoID = gdlRsp.RepoId;
                LogTrace.TraceVerbose("Retrieve Repro Id [{0}]",strRepoID);
            }
            else
            {
                LogTrace.TraceError("Retrieve Repro ID failed.");
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }

            HttpSeaFileGetDirectoryEntriesResponse gdresp =
                    httpClient.ExecuteSeaFileGetDirectoryEntriesRequest(strToken, strRepoID, strCurrentFolder);

            if (gdresp != null)
            {
                ListDirectory(gdresp);
            }
            else
            {
                LogTrace.TraceError("Get Default Directory list failed.");
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void lvFilelist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int iSelected = lvFilelist.SelectedItems.Count;

            // Double click should only process one item
            if (iSelected == 1)
            {
                ListViewItem item = lvFilelist.SelectedItems[0];
                LogTrace.TraceInfo("Selected item [{0}:{1}]",item.Name,item.Tag);

                // If selected item is folder, list the items in the folder
                if (item.Tag.ToString() == "dir" || item.Tag.ToString() == "dirback")
                {
                    if (item.Tag.ToString() == "dir")
                    {
                        lstPreviousfolder.Add(strCurrentFolder);
                        LogTrace.TraceInfo("Add current folder [{0}] into previous folder list.", strCurrentFolder);

                        strCurrentFolder = strCurrentFolder + "/" + item.Text;
                        LogTrace.TraceInfo("Current folder changed to [{0}].", strCurrentFolder);
                        
                    }
                    else
                    {
                        int iPrev = lstPreviousfolder.Count;
                        if (iPrev > 0)
                        {
                            strCurrentFolder = lstPreviousfolder[iPrev - 1];
                            LogTrace.TraceInfo("Retrieve current folder [{0}] from previous folder list.", strCurrentFolder);
                            
                            lstPreviousfolder.RemoveAt(iPrev - 1);
                        }
                    }

                    HttpSeaFileGetDirectoryEntriesResponse gdResp =
                            httpClient.ExecuteSeaFileGetDirectoryEntriesRequest(strToken, strRepoID, strCurrentFolder);

                    lvFilelist.Items.Clear();

                    ListDirectory(gdResp);

                } //dir process
                else if (item.Tag.ToString() == "file")
                {
                    strSelectedFile = strCurrentFolder + "/" + item.Text;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}
