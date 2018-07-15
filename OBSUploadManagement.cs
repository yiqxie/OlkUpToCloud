using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace FileToUpload
{
    enum UploadStatus { Succeed = 200, Failed = 205, Canceled = 206, Uploading = 201}

    class DownloadPage
    {
        private String strPageContent = String.Empty;
        private String strFileName = String.Empty;

        public String PageContent
        {
            get {
                return strPageContent;
            }

            set {
                strPageContent = value;
            }
        }

        public String FileName
        {
            get
            {
                return strFileName;
            }

            set
            {
                strFileName = value;
            }
        }
    }

    /// <summary>
    /// 定义FileInfo类
    /// </summary>
    class FileInfo
    {
        private String strFileName = String.Empty;
        private String strFilePath = String.Empty;
        private String strUploadUser = String.Empty;
        //private int lFileLength = 0;
        //private String strFileHash = String.Empty;
        private FileType fileType = new FileType();
        private FileStream stream = null;
        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

        public FileInfo(String FileName, String FilePath)
        {
            strFileName = FileName;
            strFilePath = FilePath;
            stream = File.OpenRead(strFilePath);
        }

        public String UploadUser
        {
            get
            {
                return Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress;             
            }
            set { strUploadUser = value; }
        }

        /// <summary>     
        /// 文件名     
        /// </summary> 
        public String FileName
        {
            get { return strFileName; }
            //set { strFileName = value; }
        }

        /// <summary>     
        /// 文件大小     
        /// </summary> 
        public int FileLength
        {
            get { return (int)stream.Length; }
            //set { lFileLength = value; }
        }

        /// <summary>     
        /// 文件Hash值     
        /// </summary> 
        public string FileHash
        {
            get { return HashFile(stream, "sha1");}
            //set { strFileHash = value; }
        }

        public FileType FileType
        {
            get {
                if (stream != null)
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);
                    String strFileClass = String.Empty;
                    byte buffer;

                    try
                    {
                        buffer = reader.ReadByte();
                        strFileClass = buffer.ToString();
                        buffer = reader.ReadByte();
                        strFileClass += buffer.ToString();
                    }
                    catch (Exception ep)
                    {
                        LogTrace.TraceException(ep);
                        strFileClass = "0000";
                    }

                    fileType.FileClass = strFileClass;
                }

                return fileType; }
            //set { fileType = value; }
        }

        /// <summary>
        /// 计算Hash值
        /// </summary>
        private string HashFile(System.IO.Stream stream, string strAlgName)
        {
            System.Security.Cryptography.HashAlgorithm algorithm;

            if (strAlgName == null)
            {
                LogTrace.TraceWarning("algName 不能为空");
            }
            if (string.Compare(strAlgName, "sha1", true) == 0)
            {
                algorithm = System.Security.Cryptography.SHA1.Create();
            }
            else
            {
                if (string.Compare(strAlgName, "md5", true) != 0)
                {
                    LogTrace.TraceWarning("algName 只能使用 sha1 或 md5");
                }
                algorithm = System.Security.Cryptography.MD5.Create();
            }

            return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");
        }
    }
}
