using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Windows.Forms;
using Wos;
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

        LogTrace logTrace = new LogTrace();
        
        public bool m_uploadingInProgress = false;

        // Occurs before the form region is displayed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void uploadWosAttachment_FormRegionShowing(object sender, System.EventArgs e)
        {
            uploadProgressBar.Hide();

        }


        // Occurs when the form region is closed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void uploadWosAttachment_FormRegionClosed(object sender, System.EventArgs e)
        {
            //MessageBox.Show("close");
        }


        /// <summary>
        /// 选择上传的文件
        /// </summary>
        private void btSelectFile_Click(object sender, EventArgs e)
        {
            String strFilePath = null;
            String strFileName = null;


            if (Globals.ThisAddIn.m_LoadProfileSucceed == false)
            {
                logTrace.TraceInfo( "Profile not cached, start first quering.");

                bool result = Globals.ThisAddIn.QueryProfile();
                if (result == false)
                {
                    MessageBox.Show("配置文件未下载成功！");
                    return;
                }
            }

            ClientProfile profile = Globals.ThisAddIn.Profile;

            OpenFileDialog dlgSelectFile = new OpenFileDialog();
            dlgSelectFile.FileName = "*.*";

            if (dlgSelectFile.ShowDialog() == DialogResult.OK)
            {
                strFilePath = dlgSelectFile.FileName;
                strFileName = dlgSelectFile.SafeFileName;
            }
            else 
            {
                return;
            }

            logTrace.TraceInfo("Select file: {0}, Path: {1}, for uploading.", strFileName, strFilePath);

            Application.DoEvents();

            //去掉该声名，进度条将无法使用
            ProgressBar pb = new ProgressBar();
            //Label lb = new Label();

            //获取文件信息
            FileInfo fInfo = GetFileInfo(strFilePath, strFileName);


            logTrace.TraceVerbose( "File {0} size is {1:N}", strFileName, fInfo.FileLength);

            //检查文件是否为空，或超过管理员限制大小
            if (fInfo.FileLength == 0)
            {
                MessageBox.Show("文件为空，请重新选择！");
                return;
            }
            else if (fInfo.FileLength/1024/1024 > profile.MaxFileLength)
            {
                MessageBox.Show(" 上传文件大小超过管理员设定最大值(" + profile.MaxFileLength + "MB)！");
                return;
            }

            
            //向Web Service发送上传文件请求
            OBSUploadManagement obsMgr = new OBSUploadManagement(Globals.ThisAddIn.m_strWebServiceUrl);
            UploadReqResponse uploadResponse = obsMgr.RequestUploadFile(fInfo);


            //如果Web Service接收了上传请求，返回oid

            if (uploadResponse.Code == 201) // 接受上传附件
            {
                logTrace.TraceInfo("Web Server allow to upload file to OBS.");

                //初始化进度条对象
                uploadProgressBar.Minimum = 0;
                uploadProgressBar.Maximum = CaculateProgressBarMaxSize(fInfo.FileLength);
                uploadProgressBar.Value = 0;
                uploadProgressBar.Show();
                btSelectFile.Enabled = false;

                //创建后台线程来上传附件到云盘
                m_bgwUploadFile = new BackgroundWorker(); // 实例化后台对象

                m_bgwUploadFile.WorkerReportsProgress = true; // 设置可以通告进度
                m_bgwUploadFile.WorkerSupportsCancellation = true; // 设置可以取消

                //注册后台线程事件
                m_bgwUploadFile.DoWork += new DoWorkEventHandler(bgwUploadFile_DoWork);
                m_bgwUploadFile.ProgressChanged += new ProgressChangedEventHandler(bgwUploadFile_ProgressChanged);
                m_bgwUploadFile.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwUploadFile_RunWorkerCompleted);

                //启动线程
                m_bgwUploadFile.RunWorkerAsync(new object[] { strFilePath, strFileName, uploadResponse });
                logTrace.TraceInfo("Start uploading thread for file {0}", strFileName);

                uploadInfo.Text = "准备上传附件：" + strFileName + " ,请稍后...";
            }
            else if (uploadResponse.Code == 200) //附件已经存在，不需要重新上传
            {
                //替换附件到邮件中
                logTrace.TraceInfo("File ({0}) already on OBS.", strFileName);
                ReplceAttachment(strFileName, uploadResponse.FileKey);
                
            }
            else //其他，请求上传附件失败
            {
                MessageBox.Show("附件上传失败，错误代码：" + uploadResponse.Code.ToString());
                logTrace.TraceError("Upload File Request Failed, ERROR Code {0}", uploadResponse.Code.ToString());
            }

            
        }

        private FileInfo GetFileInfo(string strFilePath, string strFileName)
        {
            //获取上传文件的大小
            FileStream stream = File.OpenRead(strFilePath);
            

            FileInfo fInfo = new FileInfo();
            fInfo.FileName = strFileName;
            fInfo.FileLength = (int)stream.Length;
            fInfo.FileHash = HashFile(stream, "sha1");

            stream.Close();

            return fInfo;
        }

        private string HashFile(System.IO.Stream stream, string strAlgName)
        { 
            System.Security.Cryptography.HashAlgorithm algorithm;

            if (strAlgName == null)
            {
                logTrace.TraceWarning( "algName 不能为空");
            }
            if (string.Compare(strAlgName, "sha1", true) == 0)
            {
                algorithm = System.Security.Cryptography.SHA1.Create();
            }
            else
            {
                if (string.Compare(strAlgName, "md5", true) != 0)
                {
                    logTrace.TraceWarning("algName 只能使用 sha1 或 md5");
                }
                algorithm = System.Security.Cryptography.MD5.Create();
            }

            return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");    
        }

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

            logTrace.TraceVerbose( "Caculate the size of bytes ({0}) that in the uploading stream each time.", iStep);

            return (int)Math.Ceiling((double)iFileLength / (double)iStep);
                 
        }


        /// <summary>
        /// 开始上传附件到OBS云端存储
        /// </summary>
        /*private void bgwUploadFile_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ClientProfile profile = Globals.ThisAddIn.Profile;

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();

            Outlook.OlObjectClass objClass = (Outlook.OlObjectClass)objInspector.CurrentItem.Class;


            String[] strArray = (String[])e.Argument;

            String strFilePath = strArray[0];
            String strFileName = strArray[1];
            String strJasonTxt = strArray[2];

            WosHttpClient httpClient = new WosHttpClient();

            httpClient.Url = "http://10.20.13.107/cmd/reserve";

            HttpWosReserveResponse reserveRsp = httpClient.ExecuteWosReserveRequest();

            //MessageBox.Show("Reserve OID: " + reserveRsp.XDdnOid);

            m_uploadingInProgress = true;

            try
            {
                httpClient.AttachFile(strFilePath, strFileName);

                httpClient.Url = "http://10.20.13.107/cmd/putoid";

                HttpWosPutOidResponse putRsp = httpClient.ExecuteBackGroundWosPutOidStream(this.m_bgwUploadFile, reserveRsp.XDdnOid);


                httpClient.Url = "http://10.20.13.107/cmd/get";

                HttpWosRetrieveMetadataResponse retRsp = httpClient.ExecuteWosRetrieveMetedateRequest(putRsp.XDdnOid);

                ((dynamic)objInspector.CurrentItem).Body = ((dynamic)objInspector.CurrentItem).Body + retRsp.XDdnOid + "\r\n";
            }
            catch (Exception ept)
            {
                MessageBox.Show(ept.Message);
                MessageBox.Show(ept.StackTrace);
                m_uploadingInProgress = false;
            }


            try
            {
                ((dynamic)objInspector.CurrentItem).Attachments.Add(strFilePath, Outlook.OlAttachmentType.olByValue, 1, strFileName);
                ((dynamic)objInspector.CurrentItem).Save();
            }
            catch (Exception ept)
            {
                MessageBox.Show("add attachment failed.  " + ept.Message);
                m_uploadingInProgress = false;
            }

            m_uploadingInProgress = false;

        }*/

        /// <summary>
        /// 开始上传附件到OBS云端存储
        /// </summary>
        private void bgwUploadFile_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ClientProfile profile = Globals.ThisAddIn.Profile;
            OBSUploadManagement obsMgr = new OBSUploadManagement(Globals.ThisAddIn.m_strWebServiceUrl);

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();


            Object[] strArray = (Object[])e.Argument;

            String strFilePath = (String)strArray[0];
            String strFileName = (String)strArray[1];
            UploadReqResponse uploadResponse = (UploadReqResponse)strArray[2];

            OBSHttpClient httpClient = new OBSHttpClient();

            m_uploadingInProgress = true;
            
            try
            {
                httpClient.AttachFile(strFilePath, strFileName);

                //uploadInfo.Text = "正在连接存储 ,请稍后...";
                
                HttpOBSPutOidResponse response = httpClient.ExecuteBackGroundOBSPutOidStream(this.m_bgwUploadFile, uploadResponse.Jason);

                if (response.XDdnStatus == System.Net.HttpStatusCode.OK)
                {
                    //更新文件上传状态到Web Service
                    bool bResult = obsMgr.UpdateUploadFileStatus(uploadResponse.FileKey, UploadStatus.Succeed);
                    logTrace.TraceInfo("Update status {0}, FileKey is {1}, Result is {2}.", UploadStatus.Succeed.ToString(), uploadResponse.FileKey,bResult);

                    //替换附件到邮件中
                    ReplceAttachment(strFileName, uploadResponse.FileKey);
                }
                else
                {
                    //更新文件上传状态到Web Service
                    bool bResult = obsMgr.UpdateUploadFileStatus(uploadResponse.FileKey, UploadStatus.Failed);
                    logTrace.TraceInfo("Update status {0}, FileKey is {1}, Result is {2}.", UploadStatus.Failed.ToString(), uploadResponse.FileKey, bResult);

                    //uploadInfo.Text = "";

                    MessageBox.Show("附件上传云端失败！");
                }
            }
            catch (Exception ept)
            {
                //更新文件上传状态到Web Service
                bool bResult = obsMgr.UpdateUploadFileStatus(uploadResponse.FileKey, UploadStatus.Failed);
                logTrace.TraceInfo("Update status {0}, FileKey is {1}, Result is {2}.", UploadStatus.Failed.ToString(), uploadResponse.FileKey, bResult);

                MessageBox.Show(ept.Message);
                MessageBox.Show(ept.StackTrace);
                logTrace.TraceException(ept);
                m_uploadingInProgress = false;
            }               

            m_uploadingInProgress = false;

        }

        private void bgwUploadFile_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            uploadProgressBar.Value = e.ProgressPercentage;
            uploadInfo.Text = "正在上传附件：" + (String)e.UserState + " ,请稍后...("+e.ProgressPercentage.ToString()+"%)";
            logTrace.TraceVerbose("Uploding file in progress, finished {0}%", e.ProgressPercentage.ToString());
        }

        private void bgwUploadFile_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            uploadProgressBar.Hide();
            uploadInfo.Text = "";
            btSelectFile.Enabled = true;               
        }

        private void ReplceAttachment(String strFileName, String strFileKey)
        {
            ClientProfile profile = Globals.ThisAddIn.Profile;
            OBSUploadManagement obsMgr = new OBSUploadManagement(Globals.ThisAddIn.m_strWebServiceUrl);

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();

            //替换附件到邮件中
            try
            {
                String strTempFolder = System.Environment.GetEnvironmentVariable("TEMP");
                String strTempFile = strTempFolder + "\\" + strFileName + ".htm";
                String strHtmlPage = obsMgr.RequestDownloadPage(strFileKey);
                FileStream stream = File.Create(strTempFile);

                StreamWriter sWrite = new StreamWriter(stream);
                sWrite.Write(strHtmlPage);
                sWrite.Close();
                stream.Close();

                logTrace.TraceVerbose("Create Temp file under: {0}", strTempFile);

                ((dynamic)objInspector.CurrentItem).Attachments.Add(strTempFile, Outlook.OlAttachmentType.olByValue, 1, strFileName);
                ((dynamic)objInspector.CurrentItem).Save();

                if (File.Exists(strTempFile))
                {
                    //如果存在则删除
                    File.Delete(strTempFile);
                    logTrace.TraceVerbose("Remove Temp file under: {0}", strTempFile);
                }
            }
            catch (Exception ept)
            {
                MessageBox.Show("Add attachment failed.  " + ept.Message);
                logTrace.TraceException(ept);
                m_uploadingInProgress = false;
            }
        }
    }
}
