using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
//using AttachmentToWos.CloudWebService;
using Microsoft.Win32;
using System.Threading;


namespace FileToUpload
{
    public partial class ThisAddIn
    {
        Outlook.Inspectors inspectors;
        private bool m_bLoadProfileSucceed = false;
        private String m_strWebServiceUrl = String.Empty;
        private String m_strDefaultFolder = String.Empty;
        private String m_strPassword = String.Empty;

        private int m_iExpire = 0;
        private int m_iMaxFileSize = 100;

        private LogLevel m_level = LogLevel.None;

        static System.Windows.Forms.Timer m_tmQueryProfile = new System.Windows.Forms.Timer();

        LogTrace m_LogTrace = new LogTrace();
        
        public bool LoadProfileSucceed
        {
            get { return m_bLoadProfileSucceed; }
            set { m_bLoadProfileSucceed = value; }
        }

        public String WebServiceUrl
        {
            get { return m_strWebServiceUrl; }
            set { m_strWebServiceUrl = value; }
        }

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

        #region Profile
        private ClientProfile m_Profile = new ClientProfile();

        /// <summary>
        /// 读取客户端配置文件
        /// </summary>
        public ClientProfile Profile
        {
            get { return m_Profile; }
            set { m_Profile = value; }
        }

        #endregion

        public void LoadProfile()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", true);

            if (regKey != null)
            {
                String[] subkeyNames = regKey.GetValueNames();

                Boolean bDefaultUploadFolder = false;
                Boolean bDefaultServerURL = false;
                Boolean bDefaultLogLevel = false;
                //this.Application.Session.ExchangeConnectionMode
                
                foreach (string keyName in subkeyNames)
                {
                    switch (keyName)
                    {
                        case "ServiceURL":
                            m_strWebServiceUrl = regKey.GetValue("ServiceURL").ToString();
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

                        case "maxFileSize":
                            m_iMaxFileSize = Convert.ToInt32(regKey.GetValue("maxFileSize").ToString());
                            break;

                        case "DefaultUploadFolder":
                            bDefaultUploadFolder = true;
                            break;

                        case "DefaultServiceURL":
                            bDefaultServerURL = true;
                            break;

                        case "DefaultLogLevel":
                            bDefaultLogLevel = true;
                            break;

                        default:
                            break;
                    }
                }

                if (m_strWebServiceUrl == null || m_strWebServiceUrl == String.Empty)
                {
                    m_strWebServiceUrl = FileToUpload.Properties.Resources.strResDefaultServiceURL;
                    regKey.SetValue("ServiceURL", m_strWebServiceUrl);
                }

                if (m_strDefaultFolder == null || m_strDefaultFolder == String.Empty)
                {
                    m_strDefaultFolder = FileToUpload.Properties.Resources.strResDefaultUploadFolder;
                    regKey.SetValue("UploadFolder", m_strDefaultFolder);
                }

                if (m_iMaxFileSize == 100)
                {
                    regKey.SetValue("maxFileSize", m_iMaxFileSize);
                }

                if (!bDefaultUploadFolder)
                {
                    regKey.SetValue("DefaultUploadFolder", FileToUpload.Properties.Resources.strResDefaultUploadFolder);
                }

                if (!bDefaultServerURL)
                {
                    regKey.SetValue("DefaultServiceURL", FileToUpload.Properties.Resources.strResDefaultServiceURL);
                }

                if (!bDefaultLogLevel)
                {
                    regKey.SetValue("DefaultLogLevel", (LogLevel)Enum.Parse(typeof(LogLevel), FileToUpload.Properties.Resources.strResDefaultLogLevel));
                }
                
            }
            else
            {
                regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin");

                if (regKey != null)
                {
                    regKey.SetValue("DefaultUploadFolder", FileToUpload.Properties.Resources.strResDefaultUploadFolder);
                    regKey.SetValue("DefaultServiceURL", FileToUpload.Properties.Resources.strResDefaultServiceURL);
                    regKey.SetValue("DefaultLogLevel", (LogLevel)Enum.Parse(typeof(LogLevel), FileToUpload.Properties.Resources.strResDefaultLogLevel));

                    regKey.SetValue("UploadFolder", FileToUpload.Properties.Resources.strResDefaultUploadFolder);
                    regKey.SetValue("ServiceURL", FileToUpload.Properties.Resources.strResDefaultServiceURL);
                    regKey.SetValue("logLevel", (LogLevel)Enum.Parse(typeof(LogLevel), FileToUpload.Properties.Resources.strResDefaultLogLevel));
                    regKey.SetValue("maxFileSize", 100);
                    regKey.SetValue("Expire", "7");
                    regKey.SetValue("ShareKey", "");

                    m_strDefaultFolder = FileToUpload.Properties.Resources.strResDefaultUploadFolder;
                    m_strWebServiceUrl = FileToUpload.Properties.Resources.strResDefaultServiceURL;
                    m_level = (LogLevel)Enum.Parse(typeof(LogLevel), FileToUpload.Properties.Resources.strResDefaultLogLevel);
                    m_iMaxFileSize = 100;
                    m_iExpire = 7;

                    //regKey.SetValue("Description", "");
                }
            }
        }

        /// <summary>
        /// 插件启动的时候，进行初始化配置
        /// </summary>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            inspectors = this.Application.Inspectors;
            inspectors.NewInspector +=
            new Microsoft.Office.Interop.Outlook.InspectorsEvents_NewInspectorEventHandler(Inspectors_NewInspector);

            this.Application.ItemSend += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_ItemSendEventHandler(ThisAddIn_Send);
            //this.Application.ItemLoad += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_ItemLoadEventHandler(ThisAddIn_Load);

            try
            {

                //GetWebServiceURL();
                //QueryProfile();
                LoadProfile();
            }
            catch (Exception ep)
            {
                WebServiceUrl = String.Empty;
                LogTrace.TraceException(ep);
            }

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true);
            if (regKey != null)
            {
                String[] strSubKeyNames = regKey.GetSubKeyNames();
                foreach (String strKeyName in strSubKeyNames)
                {
                    RegistryKey regSubKey = regKey.OpenSubKey(strKeyName);
                    try
                    {
                        if (regSubKey.GetValue("DisplayName").ToString().Trim() == "PinganAttachment.OutlookAddin")
                        {
                            regKey.DeleteSubKeyTree(strKeyName);
                        }
                    }
                    catch
                    {}
                }
            }

            //m_tmQueryProfile.Tick += TimerQueryProfile_Tick;
        }

        /// <summary>
        /// 从注册表中获取Web Service的URL地址
        /// </summary>
        private void GetWebServiceURL()
        {
            try
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\OFFICE\OUTLOOK\ADDINS\PinganAttachment.OutlookAddin", false);
                WebServiceUrl = regKey.GetValue("WosAppServiceURL").ToString();
                LogTrace.TraceVerbose("Read web service URL({0}) from register table.", WebServiceUrl);
            }
            catch (Exception ept)
            {
                WebServiceUrl = String.Empty;
                LogTrace.TraceException(ept);
            }
        }

        /// <summary>
        /// 从注册表中获取Web Service的URL地址
        /// </summary>
        public String GetSeafileURL()
        {
            return m_strWebServiceUrl;
        }

        public String GetUploadFolder()
        {
            return m_strDefaultFolder;
        }

        public String GetShareFilePassword()
        {
            return m_strPassword;
        }

        public LogLevel GetLogLevel()
        {
            return (LogLevel)m_level;
        }

        public int GetExpireDates()
        {
            return m_iExpire;
        }

        public int GetMaxFileSize()
        {
            return m_iMaxFileSize;
        }

        public void RefeshProfile()
        {
            LoadProfile();
        }

        /// <summary>
        /// 定时器，定时获取客户端配置信息
        /// </summary>
        void TimerQueryProfile_Tick(object sender, EventArgs e)
        {
            //bool result = this.QueryProfile();
            LogTrace.TraceVerbose("Timer for Profile query triggered.");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }


        void Inspectors_NewInspector(Microsoft.Office.Interop.Outlook.Inspector Inspector)
        {

        }

        public void ThisAddIn_Open(ref bool Cancel)
        {

        }


        /// <summary>
        /// 在用户发送邮件时检查附件是否上传完成
        /// </summary>
        public void ThisAddIn_Send(Object Item, ref bool Cancel)
        {
            WindowFormRegionCollection formRegions =
            Globals.FormRegions
                [Globals.ThisAddIn.Application.ActiveInspector()];

            if (formRegions.uploadWosAttachment.UploadInProgress)
            {
                DialogResult result =
                    MessageBox.Show("有附件正在上传，是否放弃？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);

                if (result == DialogResult.Cancel)
                {
                    Cancel = true;
                    return;
                }
                LogTrace.TraceInfo("Send item and discard attachment uploading.");
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }

    /// <summary>
    /// 定义ClientProfile类
    /// 可扩充
    /// </summary>
    public class ClientProfile
    {
        private String strProfileVersion = String.Empty;
        private String strDownloadPageURL = String.Empty;
        private String strNoSupportExt = String.Empty;
        private String strInfo = String.Empty;

        private long lMaxFileLength = 0;
        private int iCheckProfilePeriod = 60;
        private int iMaxRetry = 3;

        bool bEnableLogging = true;
        LogLevel tLogLevel = LogLevel.Verbose;

        //配置版本
        public string ProfileVersion
        {
            get { return strProfileVersion; }
            set { strProfileVersion = value; }
        }

        //下载链接
        public string DownloadPageURL
        {
            get { return strDownloadPageURL; }
            set { strDownloadPageURL = value; }
        }

        //客户端提示信息
        public string Info
        {
            get { return strInfo; }
            set { strInfo = value; }
        }

        //最大允许上传附件大小，单位MB
        public long MaxFileLength
        {
            get { return lMaxFileLength; }
            set { lMaxFileLength = value; }
        }

        //更新客户端配置间隔时间
        public int CheckProfilePeriod
        {
            get { return iCheckProfilePeriod; }
            set { iCheckProfilePeriod = value; }
        }

        //客户端配置更新失败重试次数
        public int MaxRetry
        {
            get { return iMaxRetry; }
            set { iMaxRetry = value; }
        }

        //配置是否允许客户端记录日志
        public bool EnableLogging
        {
            get { return bEnableLogging; }
            set { bEnableLogging = value; }
        }

        //日志类别
        public LogLevel LogLevel
        {
            get { return tLogLevel; }
            set { tLogLevel = value; }
        }

        public String NotSupportedExt
        {
            set { strNoSupportExt = value; }
        }

        public Boolean QueryExt(String strExt)
        {
            String[] strArrayExt = strNoSupportExt.Split(';');
            foreach (String strSub in strArrayExt)
            {
                if (strExt.CompareTo(strSub) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
