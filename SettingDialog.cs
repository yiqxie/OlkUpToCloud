using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace FileToUpload
{
    partial class SettingDialog : Form
    {
        private String m_strServerURL = String.Empty;
        private String m_strDefaultFolder = String.Empty;
        private String m_strPassword = String.Empty;
        private LogLevel m_level = LogLevel.None;
        private int m_iExpire = -1;

        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

        public SettingDialog()
        {
            InitializeComponent();
            //this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            if (regKey != null)
            {
                try
                {
                    this.textBoxDescription.Lines = regKey.GetValue("Description").ToString().Replace(@"\n","\n").Split('\n');
                }
                catch
                {
                    this.textBoxDescription.Lines = "1、平安邮件大附件上传插件可支持上传最大不超过100MB的文件\n2、平安邮件大附件提取页面有效期最长为7天，失效后可联系发件人申请从其网盘中选择该文件分享".Split('\n');
                }
            }
            else
            {
                this.textBoxDescription.Lines = "1、平安邮件大附件上传插件可支持上传最大不超过100MB的文件\n2、平安邮件大附件提取页面有效期最长为7天，失效后可联系发件人申请从其网盘中选择该文件分享".Split('\n');
            }
            cbLogLevel.SelectedIndex = 0;

            LoadProfile();
            CheckLogon();
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

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

        public void CheckLogon(/*object source, System.Timers.ElapsedEventArgs e*/)
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);
            String strDesKey = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Substring(0, 8);

            String m_strToken = String.Empty;

            if (regKey != null)
            {
                try
                {
                    m_strToken = Decrypt(regKey.GetValue("token").ToString(), strDesKey);
                }
                catch (Exception ept)
                {
                    LogTrace.TraceException(ept);
                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    return;
                }
            }
            else
            {
                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                return;
            }

            if (m_strToken == null || m_strToken == String.Empty)
            {
                pbLogon.Image = global::FileToUpload.Properties.Resources.off;
            }
            else
            {
                WosHttpClient httpClient = null;
                string strUrl = Globals.ThisAddIn.GetSeafileURL();

                if (strUrl != null && strUrl != "" && strUrl != String.Empty)
                {
                    httpClient = new WosHttpClient(strUrl);
                    LogTrace.TraceInfo("Connect to Fileshare with URL: {0}", strUrl);
                }
                else
                {
                    LogTrace.TraceError("Sharefile URL is empty, turn logon off");
                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    return;
                }

                try
                {
                    Boolean bCheckLogon = httpClient.ExecuteSeaFilePingRequest(m_strToken);

                    if (bCheckLogon)
                    {
                        LogTrace.TraceInfo("Turn logon on (1)");
                        pbLogon.Image = global::FileToUpload.Properties.Resources.on;
                    }
                    else
                    {
                        LogTrace.TraceInfo("Turn logon off (1)");
                        pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    }
                }
                catch (Exception ept)
                {
                    LogTrace.TraceInfo("Turn logon off (1)");
                    pbLogon.Image = global::FileToUpload.Properties.Resources.off;
                    LogTrace.TraceException(ept);
                }

            }
        }  

        private void btConfirm_Click(object sender, EventArgs e)
        {
            m_strServerURL = tbServerURL.Text;
            m_strPassword = tbPassword.Text;
            m_strDefaultFolder = tbDefaultFolder.Text;
            m_level = (LogLevel)cbLogLevel.SelectedIndex;
            m_iExpire = Convert.ToInt32(nudExpire.Value);

            if (m_iExpire <= 0 || m_iExpire > 7)
            {
                m_iExpire = 7;
            }

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            if (regKey != null)
            {
                regKey.SetValue("UploadFolder", m_strDefaultFolder);
                regKey.SetValue("ServiceURL", m_strServerURL);
                regKey.SetValue("ShareKey", m_strPassword);
                regKey.SetValue("Expire", m_iExpire.ToString());
                regKey.SetValue("logLevel", ((int)m_level).ToString());
            }

            this.Close();
        }


        void LoadProfile()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            if (regKey != null)
            {
                String[] subkeyNames = regKey.GetValueNames();

                foreach (string keyName in subkeyNames)
                {
                    switch (keyName)
                    { 
                        case "ServiceURL":
                            m_strServerURL = regKey.GetValue("ServiceURL").ToString();
                            break;

                        case "UploadFolder":
                            m_strDefaultFolder = regKey.GetValue("UploadFolder").ToString();
                            break;

                        case "ShareKey":
                            m_strPassword = regKey.GetValue("ShareKey").ToString();
                            break;

                        case "Expire":
                            m_iExpire = Convert.ToInt32(regKey.GetValue("Expire").ToString());
                            break;

                        case "logLevel":
                            m_level = (LogLevel)Enum.Parse(typeof(LogLevel), regKey.GetValue("logLevel").ToString());
                            break;

                        default:
                            break;
                    }
                }

                //
                if (m_strDefaultFolder == String.Empty)
                {
                    regKey.SetValue("UploadFolder", "");
                }

                if (m_strServerURL == String.Empty)
                {
                    regKey.SetValue("ServiceURL", "");
                }

                if (m_strPassword == String.Empty)
                {
                    regKey.SetValue("ShareKey", "");
                }

                if (m_iExpire == -1 || m_iExpire <= 0 || m_iExpire > 7)
                {
                    regKey.SetValue("Expire", "7");
                    m_iExpire = 7;
                }

                regKey.SetValue("logLevel", m_level);
            }

            tbServerURL.Text = m_strServerURL;
            tbPassword.Text = m_strPassword;
            tbDefaultFolder.Text = m_strDefaultFolder;
            cbLogLevel.SelectedIndex = (int)m_level;
            nudExpire.Value = Convert.ToDecimal(m_iExpire);
        }

        private void btDefault_Click(object sender, EventArgs e)
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);
         
            if (regKey != null)
            {
                String[] strKeyNames = regKey.GetValueNames();

                Boolean bLevel = false;

                foreach(String strKeyName in strKeyNames)
                {
                    switch(strKeyName)
                    {
                        case "DefaultUploadFolder":
                            m_strDefaultFolder = regKey.GetValue("DefaultUploadFolder").ToString();
                            break;

                        case "DefaultServiceURL":
                            m_strServerURL = regKey.GetValue("DefaultServiceURL").ToString();
                            break;

                        case "DefaultLogLevel":
                            bLevel = true;
                            m_level = (LogLevel)Enum.Parse(typeof(LogLevel),regKey.GetValue("DefaultLogLevel").ToString());
                            break;
                    }
                    
                }

                if(m_strDefaultFolder == null || m_strDefaultFolder == String.Empty)
                {
                    m_strDefaultFolder = FileToUpload.Properties.Resources.strResDefaultUploadFolder;
                    regKey.SetValue("DefaultUploadFolder", m_strDefaultFolder);
                }

                if (m_strServerURL == null || m_strServerURL == String.Empty)
                {
                    m_strServerURL = FileToUpload.Properties.Resources.strResDefaultServiceURL;
                    regKey.SetValue("DefaultServiceURL", m_strServerURL);
                }

                if (!bLevel)
                {
                    m_level = (LogLevel)Enum.Parse(typeof(LogLevel),FileToUpload.Properties.Resources.strResDefaultLogLevel);
                    regKey.SetValue("DefaultLogLevel", m_level.ToString());
                }
               
                tbDefaultFolder.Text = m_strDefaultFolder;
                tbServerURL.Text = m_strServerURL;
                cbLogLevel.SelectedIndex = (int)m_level;

                regKey.SetValue("UploadFolder", m_strDefaultFolder);
                regKey.SetValue("ServiceURL", m_strServerURL);
                regKey.SetValue("logLevel", ((int)m_level).ToString());
            }
        }
    }
}
