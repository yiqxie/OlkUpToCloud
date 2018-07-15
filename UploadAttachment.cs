using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Windows.Forms;
//using Wos;
using System.Threading;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
//using AttachmentToWos.CloudWebService;
using System.Security.Cryptography;
using System.Diagnostics;

//using OBS;

namespace FileToUpload
{
    partial class UploadWosAttachment
    {
        #region Form Region Factory

        [Microsoft.Office.Tools.Outlook.FormRegionMessageClass(Microsoft.Office.Tools.Outlook.FormRegionMessageClassAttribute.Appointment)]
        [Microsoft.Office.Tools.Outlook.FormRegionMessageClass(Microsoft.Office.Tools.Outlook.FormRegionMessageClassAttribute.Note)]
        [Microsoft.Office.Tools.Outlook.FormRegionName("AttachmentToWos.uploadWosAttachment")]
        public partial class uploadWosAttachmentFactory
        {
            // Occurs before the form region is initialized.
            // To prevent the form region from appearing, set e.Cancel to true.
            // Use e.OutlookItem to get a reference to the current Outlook item.
            private void uploadWosAttachmentFactory_FormRegionInitializing(object sender, Microsoft.Office.Tools.Outlook.FormRegionInitializingEventArgs e)
            {

            }
        }

        #endregion

        private BackgroundWorker m_bgwUploadFile;

        private String m_strToken = String.Empty;

        private String m_strRepoID = String.Empty;

        private String m_strDefaultUploadFolder = String.Empty;

        private Boolean m_bUploadFolder = false;

        private WosHttpClient httpClient = null;

        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

        public bool m_bUploadInProgress = false;

        private bool m_bRegisterWindowClose = false;

        public bool UploadInProgress
        {
            get { return m_bUploadInProgress; }
            set { m_bUploadInProgress = value; }
        }

        // Occurs before the form region is displayed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void uploadWosAttachment_FormRegionShowing(object sender, System.EventArgs e)
        {
            uploadProgressBar.Hide();

            lbInfo.Show();

            lbInfo.Text = Globals.ThisAddIn.Profile.Info;

            pbSetting.Image = global::FileToUpload.Properties.Resources.gear_gold;
            pbLogon.Image = global::FileToUpload.Properties.Resources.off;
            pbSetting.Left = this.Width - 50;
            pbLogon.Left = this.Width - 80;
            m_strDefaultUploadFolder = Globals.ThisAddIn.GetUploadFolder();

            //CheckLogon();

            //System.Timers.Timer tLogon = new System.Timers.Timer(5000);
            //tLogon.Elapsed += new System.Timers.ElapsedEventHandler(CheckLogon);   
            //tLogon.AutoReset = true;
            //tLogon.Enabled = true;  

        }

        //public void CheckLogon(/*object source, System.Timers.ElapsedEventArgs e*/)
        //{
        //    RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\14.0\Outlook\Addins\FileToUpload", true);
        //    String strDesKey = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Substring(0,8);

        //    if (regKey != null)
        //    {
        //        try
        //        {
        //            m_strToken = Decrypt(regKey.GetValue("token").ToString(), strDesKey);
        //        }
        //        catch (Exception ept)
        //        {
        //            LogTrace.TraceException(ept);
        //            pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //            return;
        //        }
        //    }
        //    else {
        //        pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //        return;
        //    }

        //    if (m_strToken == null || m_strToken == String.Empty)
        //    {
        //        pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //    }
        //    else
        //    {
        //        if (httpClient == null)
        //        {
        //            string strUrl = Globals.ThisAddIn.GetSeafileURL();

        //            if (strUrl != null && strUrl != "" && strUrl != String.Empty)
        //            {
        //                httpClient = new WosHttpClient(strUrl);
        //                LogTrace.TraceInfo("Connect to Fileshare with URL: {0}", strUrl);
        //            }
        //            else
        //            {
        //                LogTrace.TraceError("Sharefile URL is empty, turn logon off");
        //                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //                return;
        //            }

        //            try
        //            {
        //                Boolean bCheckLogon = httpClient.ExecuteSeaFilePingRequest(m_strToken);

        //                if (bCheckLogon)
        //                {
        //                    LogTrace.TraceInfo("Turn logon on (1)");
        //                    pbLogon.Image = global::FileToUpload.Properties.Resources.on;
        //                }
        //                else
        //                {
        //                    LogTrace.TraceInfo("Turn logon off (1)");
        //                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //                }
        //            }
        //            catch (Exception ept)
        //            {
        //                LogTrace.TraceInfo("Turn logon off (1)");
        //                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //                LogTrace.TraceException(ept);
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                Boolean bCheckLogon = httpClient.ExecuteSeaFilePingRequest(m_strToken);

        //                if (bCheckLogon)
        //                {
        //                    LogTrace.TraceInfo("Turn logon on (2)");
        //                    pbLogon.Image = global::FileToUpload.Properties.Resources.on;
        //                }
        //                else
        //                {
        //                    LogTrace.TraceInfo("Turn logon off (2)");
        //                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //                }
        //            }
        //            catch (Exception ept)
        //            {
        //                LogTrace.TraceInfo("Turn logon off (2)");
        //                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
        //                LogTrace.TraceException(ept);
        //            }
        //        }
        //    }
        //}  

        // Occurs when the form region is closed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void uploadWosAttachment_FormRegionClosed(object sender, System.EventArgs e)
        {
            //如果有附件在上传，则取消上传
            if (m_bUploadInProgress)
            {
                m_bgwUploadFile.CancelAsync();
                m_bUploadInProgress = false;
            }
        }


        /// <summary>
        /// 选择上传的文件
        /// </summary>
        private void btSelectFile_Click(object sender, EventArgs e)
        {
            String strFilePath = String.Empty;
            String strFileName = String.Empty;
            String strFileExt = String.Empty;

            String[] strArrFilePath;
            String[] strArrFileName;

            OpenFileDialog dlgSelectFile = new OpenFileDialog();
            dlgSelectFile.FileName = "*.*";
            dlgSelectFile.Multiselect = true;

            //打开文件选择对话框
            if (dlgSelectFile.ShowDialog() == DialogResult.OK)
            {
                strFilePath = dlgSelectFile.FileName;
                strFileName = dlgSelectFile.SafeFileName;
                strFileExt = Path.GetExtension(strFileName);

                strArrFileName = dlgSelectFile.SafeFileNames;
                strArrFilePath = dlgSelectFile.FileNames;
            }
            else
            {
                return;
            }

            LogTrace.TraceInfo("Select file: {0}, Path: {1}, for uploading.", strFileName, strFilePath);

            //去掉该声名，进度条将无法使用
            ProgressBar pb = new ProgressBar();

            //LogTrace.TraceVerbose("File {0} size is {1:N}", strFileName, fInfo.FileLength);

            try
            {
                LogTrace.TraceInfo("Web Server allow to upload file to Cloud.");

                //初始化进度条对象
                //uploadProgressBar.Minimum = 0;
                //uploadProgressBar.Maximum = CaculateProgressBarMaxSize(fInfo.FileLength);
                //uploadProgressBar.Value = 0;               
                btSelectFile.Enabled = false;

                //调整控件位置
                uploadProgressBar.Width = this.Width / 3;
                uploadInfo.Left = uploadProgressBar.Right + 10;

                //创建后台线程来上传附件到云盘
                m_bgwUploadFile = new BackgroundWorker(); // 实例化后台对象

                m_bgwUploadFile.WorkerReportsProgress = true; // 设置可以通告进度
                m_bgwUploadFile.WorkerSupportsCancellation = true; // 设置可以取消

                //注册后台线程事件
                m_bgwUploadFile.DoWork += new DoWorkEventHandler(bgwUploadFile_DoWork);
                m_bgwUploadFile.ProgressChanged += new ProgressChangedEventHandler(bgwUploadFile_ProgressChanged);
                m_bgwUploadFile.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwUploadFile_RunWorkerCompleted);
                m_bgwUploadFile.WorkerSupportsCancellation = true;

                //启动线程
                m_bgwUploadFile.RunWorkerAsync(new object[] { strArrFilePath, strArrFileName/*, uploadResponse*/ });
                LogTrace.TraceInfo("Start uploading thread for file {0}", strFileName);

                uploadInfo.Text = String.Format("准备上传附件：{0} ,请稍后...", strFileName);
            }
            catch (Exception exp)
            {
                LogTrace.TraceException(exp);
                MessageBox.Show("上传附件请求失败！", "错误");
            }

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

        private Boolean SeafileLogon()
        {
            Boolean bFoundToken = false;

            string strUrl = Globals.ThisAddIn.GetSeafileURL();

            if (strUrl != null && strUrl != "" && strUrl != String.Empty)
            {
                httpClient = new WosHttpClient(strUrl);
                LogTrace.TraceInfo("Connect to Fileshare with URL: {0}", strUrl);
            }
            else
            {
                LogTrace.TraceError("Sharefile URL is empty.");
                LogTrace.TraceInfo("Turn logon off (3)");
                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                return false;
            }

            String strDesKey = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Substring(0,8);
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            try
            {
                if (regKey != null)
                {
                    m_strToken = Decrypt(regKey.GetValue("token").ToString(),strDesKey);

                    if (m_strToken != null && m_strToken != String.Empty)
                    {
                        Boolean b = httpClient.ExecuteSeaFilePingRequest(m_strToken);

                        if (b)
                        {
                            LogTrace.TraceVerbose("Retrieve Token {0} from register Key.", m_strToken);
                            bFoundToken = true;
                        }
                    }
                }
                else
                {
                    bFoundToken = false;
                }
            }
            catch (Exception ept)
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

                    LogTrace.TraceVerbose("Logon user Name is {0}, password is {1}", strLoginUser, "*********");
                }
                else
                {
                    LogTrace.TraceInfo("Cancel logon");
                    LogTrace.TraceInfo("Turn logon off (4)");
                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    return false;
                }

                HttpSeaFileTokenResponse rsp = httpClient.ExecuteSeaFileTokenRequest(strLoginUser, strLoginPassword);
                if (rsp != null)
                {
                    regKey.SetValue("token", Encrypt(rsp.Token,strDesKey));
                    m_strToken = rsp.Token;
                    LogTrace.TraceVerbose("Save token {0} into register key.", m_strToken);
                }
                else
                {
                    MessageBox.Show("1、如果您已是云盘用户，请输入正确的邮箱密码（开机密码）\n\r2、如果您还不是云盘用户请联系所在机构IT咨询如何申请开通，不要重复尝试登录避免邮箱帐号被锁定", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogTrace.TraceInfo("Turn logon off (5)");
                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    return false;
                }
            }

            HttpSeaFileGetDefaultLibraryResponse gdlRsp = httpClient.ExecuteSeaFileGetDefaultLibraryRequest(m_strToken);

            if (gdlRsp != null)
            {
                m_strRepoID = gdlRsp.RepoId;
                LogTrace.TraceVerbose("Retrieve Repro Id {0}.", m_strRepoID);
                LogTrace.TraceInfo("Turn logon on (6)");
                pbLogon.Image = global::FileToUpload.Properties.Resources.on;
                return true;
            }
            else
            {
                LogTrace.TraceError("Retrieve Repro Id failed.");
            }

            LogTrace.TraceInfo("Turn logon off (7)");
            pbLogon.Image = global::FileToUpload.Properties.Resources.off;
            return false;
        }

        /// <summary>
        /// 计算文件分块大小
        /// </summary>
        private int CaculateProgressBarMaxSize(int iFileLength)
        {
            int iStep = 0;

            if (iFileLength <= 5242880)
            {
                iStep = 204800;
            }
            else if (iFileLength <= 10485760)
            {
                iStep = 512000;
            }
            else if (iFileLength <= 20971520)
            {
                iStep = 1048576;
            }
            else
            {
                iStep = 2097152;
            }

            LogTrace.TraceVerbose("Caculate the size of bytes ({0}) that in the uploading stream each time.", iStep);

            return (int)Math.Ceiling((double)iFileLength / (double)iStep);

        }

        /// <summary>
        /// 在关闭邮件编辑窗口时进行一些处理
        /// </summary>
        public void MailItem_Close(ref bool Cancel)
        {
            if (UploadInProgress)
            {
                DialogResult result =
                    MessageBox.Show("有附件正在上传，是否继续退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);

                if (result == DialogResult.Cancel)
                {
                    Cancel = true;
                    return;
                }

                m_bgwUploadFile.CancelAsync();
                m_bUploadInProgress = false;

                LogTrace.TraceInfo("Close item edit window and discard attachment uploading.");
            }
        }

        /// <summary>
        /// 开始上传附件到OBS云端存储
        /// </summary>
        private void bgwUploadFile_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (m_bRegisterWindowClose == false)
            {
                ((Outlook.ItemEvents_10_Event)(dynamic)Globals.ThisAddIn.Application.ActiveInspector().CurrentItem).Close +=
                                new Microsoft.Office.Interop.Outlook.ItemEvents_10_CloseEventHandler(MailItem_Close);

                m_bRegisterWindowClose = true;
            }

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();

            //获取入参
            Object[] objArray = (Object[])e.Argument;

            String[] strArrFilePath = (String[])objArray[0];
            String[] strArrFileName = (String[])objArray[1];


            if (!SeafileLogon())
            {
                LogTrace.TraceError("Seafile Logon failed.");
                return;
            }

            UploadInProgress = true;

            for (int i = 0; i < strArrFilePath.Count<String>(); i++)
            {
                String strFilePath = strArrFilePath[i];
                String strFileName = strArrFileName[i];

                LogTrace.TraceInfo("Start upload file {0} : {1}", strFileName, strFilePath);

                //获取文件信息
                FileInfo fInfo = new FileInfo(strFileName, strFilePath);

                if (fInfo.FileLength > Globals.ThisAddIn.GetMaxFileSize() * 1024 * 1024)
                {
                    MessageBox.Show("文件" + strFileName + "大小超过管理员规定(" + Globals.ThisAddIn.GetMaxFileSize().ToString() + ")", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }

                if (uploadProgressBar.InvokeRequired)
                {
                    uploadProgressBar.Invoke(new MethodInvoker(delegate
                    {
                        uploadProgressBar.Minimum = 0;
                        uploadProgressBar.Maximum = CaculateProgressBarMaxSize(fInfo.FileLength);
                        uploadProgressBar.Value = 0;

                        LogTrace.TraceInfo("Reset ProgressBar bar, Min {0} Max {1} value {2}", uploadProgressBar.Minimum, uploadProgressBar.Maximum, uploadProgressBar.Value);
                    }));

                }

                if (!m_bUploadFolder)
                {
                    HttpSeaFileGetDirectoryEntriesResponse gdeRsp = httpClient.ExecuteSeaFileGetDirectoryEntriesRequest(m_strToken, m_strRepoID, "/" + m_strDefaultUploadFolder);

                    if (gdeRsp == null)
                    {
                        m_bUploadFolder = httpClient.ExecuteCreateSeaFileCreateNewDirectoryRequest(m_strToken, m_strRepoID, "/" + m_strDefaultUploadFolder);
                    }
                }

                HttpSeaFileGetUpdateLinkResponse gulDlg = httpClient.ExecuteSeaFileGetUpdateLinkRequest(m_strToken, m_strRepoID);

                if (gulDlg != null)
                {
                    //upload File to ShareFile
                    httpClient.AttachFile(strFilePath, strFileName);
                    String strResult = httpClient.ExecuteBackGroundShareFileUploadFileStream(m_bgwUploadFile, m_strToken, gulDlg.URL, "/" + m_strDefaultUploadFolder);
                    if (strResult == null || strResult == String.Empty)
                    {
                        MessageBox.Show("上传文件 " + strFileName + " 失败!");
                        LogTrace.TraceError("Upload file {0} failed.", strFileName);
                    }
                    else
                    {

                        String strDownloadLink = httpClient.ExecuteCreateSeaFileCreateDownloadLinkRequest(m_strToken, m_strRepoID, "/" + m_strDefaultUploadFolder + "/" + strFileName, "", 0);

                        if (strDownloadLink != null && strDownloadLink != "" && strDownloadLink != String.Empty)
                        {
                            LogTrace.TraceInfo("Replace attachment {0} with Link {1}", strFileName, strDownloadLink);
                            ReplceAttachment(strFileName, strDownloadLink);
                        }
                        else
                        {
                            MessageBox.Show("文件(" + strFileName + ")下载链接获取失败");
                            LogTrace.TraceError("Get Download Link for file {0} is empty", strFileName);
                        }
                    }
                }
                else
                {
                    LogTrace.TraceError("Retrieve Upload URL failed.");
                }
            }

            UploadInProgress = false;
        }

        /// <summary>
        /// 在上传过程中更新进度条状态
        /// </summary>
        private void bgwUploadFile_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

            try
            {
                uploadProgressBar.Show();
                lbInfo.Hide();
                uploadProgressBar.Value = e.ProgressPercentage;
                float fProgress = ((float)e.ProgressPercentage * (float)100 / (float)uploadProgressBar.Maximum);
                uploadInfo.Text = String.Format("正在上传附件：{0}, 完成 {1:F2}%", (String)e.UserState, fProgress);
                LogTrace.TraceVerbose("Uploding file in progress, finished {0}%", fProgress);
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
            }
        }

        /// <summary>
        /// 上传文件完成
        /// </summary>
        private void bgwUploadFile_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            uploadProgressBar.Hide();
            uploadInfo.Text = "";
            lbInfo.Show();
            btSelectFile.Enabled = true;
        }

        /// <summary>
        /// 替换附件为下载页面(HTML文件)
        /// </summary>
        private void ReplceAttachment(String strFileName, String strDownloadURL)
        {
            //ClientProfile profile = Globals.ThisAddIn.Profile;
            //OBSUploadManagement obsMgr = new OBSUploadManagement(Globals.ThisAddIn.WebServiceUrl);

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();

            //替换附件到邮件中
            try
            {
                String strTempFolder = System.Environment.GetEnvironmentVariable("TEMP");
                DownloadPage htmlPage = new DownloadPage();
                htmlPage.FileName = strFileName;
                htmlPage.PageContent = "<script language=\"javascript\" type=\"text/javascript\"> window.location.href='" + strDownloadURL + "'; </script>";
                String strTempFile = strTempFolder + @"\" + htmlPage.FileName + ".htm";
                FileStream stream = File.Create(strTempFile);

                StreamWriter sWrite = new StreamWriter(stream);
                sWrite.Write(htmlPage.PageContent);
                sWrite.Close();
                stream.Close();

                LogTrace.TraceVerbose("Create Temp file under: {0}", strTempFile);

                ((dynamic)objInspector.CurrentItem).Attachments.Add(strTempFile, Outlook.OlAttachmentType.olByValue, 1, strFileName);
                ((dynamic)objInspector.CurrentItem).Save();

                if (File.Exists(strTempFile))
                {
                    //如果存在则删除
                    File.Delete(strTempFile);
                    LogTrace.TraceVerbose("Remove Temp file under: {0}", strTempFile);
                }
            }
            catch (Exception ept)
            {
                MessageBox.Show("Add attachment failed.  " + ept.Message);
                LogTrace.TraceException(ept);
                UploadInProgress = false;
            }
        }

        private void UploadWosAttachment_ClientSizeChanged(object sender, EventArgs e)
        {
            //调整控件位置
            int iThisAddinWidth = this.Width;
            lbInfo.Left = this.Width - lbInfo.Width - 18;

            uploadProgressBar.Width = this.Width / 3;
            uploadInfo.Left = uploadProgressBar.Right + 10;

            pbSetting.Left = this.Width - 50;
            pbLogon.Left = this.Width - 80;
        }

        private void btTest_Click(object sender, EventArgs e)
        {
            System.Security.Principal.WindowsIdentity id = System.Security.Principal.WindowsIdentity.GetCurrent();
            String strDesKey = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Substring(0,8);

            ShareFileSelectDialog dlg = new ShareFileSelectDialog();
            dlg.Show();

        }

        private void btSelectShareFile_Click(object sender, EventArgs e)
        {
            String strSelectedFile = String.Empty;

            ShareFileSelectDialog dlgFileSelect = new ShareFileSelectDialog();
            if (dlgFileSelect.ShowDialog() == DialogResult.OK)
            {
                strSelectedFile = dlgFileSelect.strSelectedFile;
            }
            else
            {
                //MessageBox.Show("读取网盘数据失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            String[] strArrSelectedFile = strSelectedFile.Split(';');

            if (!SeafileLogon())
            {
                LogTrace.TraceError("Fileshare logon failed.");
                return;
            }

            foreach (String strFile in strArrSelectedFile)
            {
                String strDownloadLink = httpClient.ExecuteCreateSeaFileCreateDownloadLinkRequest(m_strToken, m_strRepoID, strFile, Globals.ThisAddIn.GetShareFilePassword(), Globals.ThisAddIn.GetExpireDates());
                String strFileName = strFile.Substring(strFile.LastIndexOf("/"));

                if (strDownloadLink != null && strDownloadLink != "" && strDownloadLink != string.Empty)
                {
                    ReplceAttachment(strFileName, strDownloadLink);

                    LogTrace.TraceInfo("Fileshare replace file {0} with Link {1} Expire time {2}", strFileName, strDownloadLink, Globals.ThisAddIn.GetExpireDates());
                }
                else
                {
                    MessageBox.Show("文件(" + strFileName + ")下载链接获取失败");
                    LogTrace.TraceError("Retrieve file {0} download link failed", strFileName);
                }
            }
        }

        private void pbSetting_Click(object sender, EventArgs e)
        {
            SettingDialog dlgSet = new SettingDialog();

            if (dlgSet.ShowDialog() == DialogResult.OK)
            {
                Globals.ThisAddIn.LoadProfile();
                //CheckLogon();
            }

        }

        private void pbLogon_Click(object sender, EventArgs e)
        {
            SeafileLogon();
        }
    }

    class FileType
    {
        private String strFileClass = String.Empty;
        private String strFileExt = String.Empty;

        //常用文件类型列表
        private String[,] strFileTypeList = new String[,] {{"4946",".txt"},
                              {"104116",".txt"},
                              {"7173",".gif"},
                              {"255216",".jpg"},
                              {"13780",".png"},
                              {"6677",".bmp"},
                              {"239187",".txt"},
                              {"239187",".aspx"},
                              {"239187",".asp"},
                              {"239187",".sql"},
                              {"208207",".xls"},
                              {"208207",".doc"},
                              {"208207",".ppt"},
                              {"6063",".xml"},
                              {"6033",".htm"},
                              {"6033",".html"},
                              {"4742",".js"},
                              {"8075",".xlsx"},
                              {"8075",".zip"},
                              {"8075",".pptx"},
                              {"8075",".mmap"},
                              {"8075",".zip"},
                              {"8297",".rar"},
                              {"01",".accdb"},
                              {"01",".mdb"},
                              {"7790",".exe"},
                              {"7790",".dll"},
                              {"5666",".psd"},
                              {"255254",".rdp"},
                              {"10056",".torrent"},
                              {"64101",".bat"},
                              {"4059",".sgf"}};

        public String FileExt
        {
            get
            {
                return strFileExt;
            }

            set
            {
                strFileExt = value;
                strFileClass = GetFileClassByExt(strFileExt);
            }
        }

        public String FileClass
        {
            get
            {
                return strFileClass;
            }

            set
            {
                strFileClass = value;
                GetFileExtByClass(strFileClass);
            }
        }

        public String GetFileExtByClass(String strClass)
        {
            strFileExt = String.Empty;
            for (int i = 0; i < strFileTypeList.Length / 2; i++)
            {
                if (strFileTypeList[i, 0].CompareTo(strClass.Trim()) == 0)
                {
                    strFileExt += strFileTypeList[i, 1] + ";";
                }
            }
            return strFileExt;
        }

        public String GetFileClassByExt(String strExt)
        {
            for (int i = 0; i < strFileTypeList.Length / 2; i++)
            {
                if (strFileTypeList[i, 1].CompareTo(strExt.Trim()) == 0)
                {
                    strFileClass += strFileTypeList[i, 0] + ";";
                }
            }
            return "0000";
        }
    }
}
