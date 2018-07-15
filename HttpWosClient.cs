using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using AttachmentToWos;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace WebClient
{
    class HttpWosClient
    {
        #region fields
        private bool keepContext;
        private string defaultLanguage = "zh-CN";
        private Encoding defaultEncoding = Encoding.UTF8;
        private string accept = "*/*";
        private string userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private HttpVerb verb = HttpVerb.GET;
        //private readonly List<HttpUploadingFile> files = new List<HttpUploadingFile>();
        private HttpUploadingFile upfile = null;
        private readonly Dictionary<string, string> postingData = new Dictionary<string, string>();
        private string url;
        private WebHeaderCollection responseHeaders;
        private int startPoint;
        private int endPoint;
        #endregion


        #region properties
        /// <summary>
        /// 是否自动在不同的请求间保留Cookie, Referer
        /// </summary>
        public bool KeepContext
        {
            get { return keepContext; }
            set { keepContext = value; }
        }

        /// <summary>
        /// 期望的回应的语言
        /// </summary>
        public string DefaultLanguage
        {
            get { return defaultLanguage; }
            set { defaultLanguage = value; }
        }

        /// <summary>
        /// GetString()如果不能从HTTP头或Meta标签中获取编码信息,则使用此编码来获取字符串
        /// </summary>
        public Encoding DefaultEncoding
        {
            get { return defaultEncoding; }
            set { defaultEncoding = value; }
        }

        /// <summary>
        /// 指示发出Get请求还是Post请求
        /// </summary>
        public HttpVerb Verb
        {
            get { return verb; }
            set { verb = value; }
        }

        /// <summary>
        /// 要发送的Form表单信息
        /// </summary>
        public Dictionary<string, string> PostingData
        {
            get { return postingData; }
        }

        /// <summary>
        /// 获取或设置请求资源的地址
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        /// <summary>
        /// 用于在获取回应后,暂时记录回应的HTTP头
        /// </summary>
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        /// <summary>
        /// 获取或设置期望的资源类型
        /// </summary>
        public string Accept
        {
            get { return accept; }
            set { accept = value; }
        }

        /// <summary>
        /// 获取或设置请求中的Http头User-Agent的值
        /// </summary>
        public string UserAgent
        {
            get { return userAgent; }
            set { userAgent = value; }
        }


        /// <summary>
        /// 获取或设置获取内容的起始点,用于断点续传,多线程下载等
        /// </summary>
        public int StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        /// <summary>
        /// 获取或设置获取内容的结束点,用于断点续传,多下程下载等.
        /// 如果为0,表示获取资源从StartPoint开始的剩余内容
        /// </summary>
        public int EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        #endregion

        #region constructors
        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        public HttpWosClient()
            : this(null)
        {
        }

        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        /// <param name="url">要获取的资源的地址</param>
        public HttpWosClient(string url)
        {
            Url = url;
        }

        #endregion

        #region AttachFile
        /// <summary>
        /// 在请求中添加要上传的文件
        /// </summary>
        /// <param name="fileName">要上传的文件路径</param>
        /// <param name="fieldName">文件字段的名称(相当于&lt;input type=file name=fieldName&gt;)里的fieldName)</param>
        public void AttachFile(string fileName, string fieldName)
        {
            //HttpUploadingFile file = new HttpUploadingFile(fileName, fieldName);
            //files.Add(file);
            upfile = new HttpUploadingFile(fileName, fieldName);
        }

        /// <summary>
        /// 在请求中添加要上传的文件
        /// </summary>
        /// <param name="data">要上传的文件内容</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fieldName">文件字段的名称(相当于&lt;input type=file name=fieldName&gt;)里的fieldName)</param>
        public void AttachFile(byte[] data, string fileName, string fieldName)
        {
            //HttpUploadingFile file = new HttpUploadingFile(data, fileName, fieldName);
            //files.Add(file);
            upfile = new HttpUploadingFile(data, fileName, fieldName);
        }
        #endregion

        /// <summary>
        /// 清空PostingData, Files, StartPoint, EndPoint, ResponseHeaders, 并把Verb设置为Get.
        /// 在发出一个包含上述信息的请求后,必须调用此方法或手工设置相应属性以使下一次请求不会受到影响.
        /// </summary>
        public void Reset()
        {
            verb = HttpVerb.GET;
            //files.Clear();
            upfile = null;
            postingData.Clear();
            responseHeaders = null;
            startPoint = 0;
            endPoint = 0;
        }

        #region WosOperation

        /// <summary>
        /// shen.
        /// 在发出一个包含上述信息的请求后,必须调用此方法或手工设置相应属性以使下一次请求不会受到影响.
        /// </summary>
        private HttpWebRequest CreateWosPutRequest()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);

            if (upfile != null)
            {
                req.Method = "POST";
                req.ContentType = "application/octet-stream";
                req.Headers.Add("x-ddn-policy: test");

                memoryStream.Write(upfile.Data, 0, upfile.Data.Length);
                writer.Flush();
            }
            else
            {
                memoryStream.Close();
                writer.Close();
                return null;
            }


            using (Stream stream = req.GetRequestStream())
            {
                memoryStream.WriteTo(stream);
            }

            memoryStream.Close();
            writer.Close();

            return req;
        }

        /// <summary>
        /// 发出一次新的WOSPut请求,并返回获得的回应
        /// 返回结果包含oid和状态信息.
        /// </summary>
        /// <returns>相应的HttpWosPutResponse</returns>
        public HttpWosPutResponse ExecuteWosPutRequest()
        {
            try
            {
                HttpWebRequest req = CreateWosPutRequest();

                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    if (res != null)
                    {
                        HttpWosPutResponse rsp = new HttpWosPutResponse();
                        rsp.XDdnOid = res.Headers.Get("x-ddn-oid");
                        rsp.XDdnStatus = res.Headers.Get("x-ddn-status");

                        return rsp;
                    }
                }
            }
            catch {
                //
                return null;
            }

            return null;
        }


        private HttpWebRequest CreateWosReserveRequest()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);

            req.Method = "POST";
            req.ContentType = "application/octet-stream";
            req.Headers.Add("x-ddn-policy: test");

            return req;
        }


        /// <summary>
        /// 发出一次新的WOSPut请求,并返回获得的回应
        /// 返回结果包含oid和状态信息.
        /// </summary>
        /// <returns>相应的HttpWosPutResponse</returns>
        public HttpWosReserveResponse ExecuteWosReserveRequest()
        {
            try
            {
                HttpWebRequest req = CreateWosReserveRequest();
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                HttpWosReserveResponse rsp = new HttpWosReserveResponse();
                rsp.XDdnOid = res.Headers.Get("x-ddn-oid");
                rsp.XDdnStatus = res.Headers.Get("x-ddn-status");

                return rsp;
            }
            catch
            {
                //
                return null;
            }
        }


        private HttpWebRequest CreateWosPutOidRequest(String oid)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);

            if (upfile != null)
            {
                req.Method = "POST";
                req.ContentType = "application/octet-stream";
                req.Headers.Add("x-ddn-policy: test");
                req.Headers.Add("x-ddn-oid: " + oid);

                memoryStream.Write(upfile.Data, 0, upfile.Data.Length);
                writer.Flush();
            }
            else
            {
                memoryStream.Close();
                writer.Close();
                return null;
            }


            using (Stream stream = req.GetRequestStream())
            {
                memoryStream.WriteTo(stream);
            }

            memoryStream.Close();
            writer.Close();


            return req;
        }


        /// <summary>
        /// 发出一次新的WOSPut请求,并返回获得的回应
        /// 返回结果包含oid和状态信息.
        /// </summary>
        /// <returns>相应的HttpWosPutResponse</returns>
        public HttpWosPutOidResponse ExecuteWosPutOidRequest(String oid)
        {
            try
            {
                HttpWebRequest req = CreateWosPutOidRequest(oid);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    HttpWosPutOidResponse rsp = new HttpWosPutOidResponse();
                    rsp.XDdnOid = res.Headers.Get("x-ddn-oid");
                    rsp.XDdnStatus = res.Headers.Get("x-ddn-status");

                    return rsp;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                //
                return null;
            }
        }

        public HttpWosPutOidResponse ExecuteBackGroundWosPutOidStream(BackgroundWorker bgw, String oid)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWosPutOidResponse rsp = new HttpWosPutOidResponse();

            if (upfile != null)
            {
                int iStep = GetStepSize(upfile.Data.Length);

                req.Method = "POST";
                req.ContentType = "application/octet-stream";
                req.Headers.Add("x-ddn-policy: test");
                req.Headers.Add("x-ddn-oid: " + oid);

                req.AllowWriteStreamBuffering = false;
                req.Timeout = 3000000;
                req.ContentLength = upfile.Data.Length;
                    
                Stream postStream = req.GetRequestStream();
                    
                for (int i = 0; i < upfile.Data.Length; i += iStep)
                {
                    postStream.Write(upfile.Data, i, (upfile.Data.Length - i) > iStep ? iStep : (upfile.Data.Length - i));
                    Thread.Sleep(1);
                    bgw.ReportProgress(i / iStep);
                }
                bgw.ReportProgress((int)Math.Ceiling((double)upfile.Data.Length / (double)iStep));

                postStream.Close();
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                rsp.XDdnOid = res.Headers.Get("x-ddn-oid");
                rsp.XDdnStatus = res.Headers.Get("x-ddn-status");
            }
            else
            {
                return null;
            }

            return rsp;
        }


        private HttpWebRequest CreateWosRetrieveMetadateRequest(String oid)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "HEAD";

            req.ContentType = "application/octet-stream";
            //req.Headers.Add("x-ddn-policy: test");
            req.Headers.Add("x-ddn-oid: " + oid);

            return req;
        }


        /// <summary>
        /// 发出一次新的WOSPut请求,并返回获得的回应
        /// 返回结果包含oid和状态信息.
        /// </summary>
        /// <returns>相应的HttpWosPutResponse</returns>
        public HttpWosRetrieveMetadataResponse ExecuteWosRetrieveMetedateRequest(String oid)
        {
            try
            {
                HttpWebRequest req = CreateWosRetrieveMetadateRequest(oid);
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                HttpWosRetrieveMetadataResponse rsp = new HttpWosRetrieveMetadataResponse();
                rsp.XDdnOid = res.Headers.Get("x-ddn-oid");
                rsp.XDdnStatus = res.Headers.Get("x-ddn-status");
                rsp.XDdnLength = Convert.ToInt32(res.Headers.Get("x-ddn-length"));
                
                return rsp;
            }
            catch
            {
                //
                return null;
            }
        }
        #endregion

        private int GetStepSize(int iFileLength)
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

            return iStep;

        }

    }

    public enum HttpVerb
    {
        GET,
        POST,
        HEAD,
    }

    public enum FileExistsAction
    {
        Overwrite,
        Append,
        Cancel,
    }

    public class HttpUploadingFile
    {
        private string fileName;
        private string fieldName;
        private byte[] data;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public HttpUploadingFile(string fileName, string fieldName)
        {
            this.fileName = fileName;
            this.fieldName = fieldName;
            using (FileStream stream = File.OpenRead(fileName)/*new FileStream(fileName, FileMode.Open)*/)
            {
                byte[] inBytes = new byte[stream.Length];
                stream.Read(inBytes, 0, inBytes.Length);
                data = inBytes;
            }
        }

        public HttpUploadingFile(byte[] data, string fileName, string fieldName)
        {
            this.data = data;
            this.fileName = fileName;
            this.fieldName = fieldName;
        }
    }

    public class HttpWosPutResponse
    {
        private string xDdnStatus;
        private string xDdnOid;

        public string XDdnStatus
        {
            get { return xDdnStatus; }
            set { xDdnStatus = value; }
        }

        public string XDdnOid
        {
            get { return xDdnOid; }
            set { xDdnOid = value; }
        }
    }


    public class HttpWosReserveResponse
    {
        private string xDdnStatus;
        private string xDdnOid;

        public string XDdnStatus
        {
            get { return xDdnStatus; }
            set { xDdnStatus = value; }
        }

        public string XDdnOid
        {
            get { return xDdnOid; }
            set { xDdnOid = value; }
        }
    }

    public class HttpWosPutOidResponse
    {
        private string xDdnStatus;
        private string xDdnOid;

        public string XDdnStatus
        {
            get { return xDdnStatus; }
            set { xDdnStatus = value; }
        }

        public string XDdnOid
        {
            get { return xDdnOid; }
            set { xDdnOid = value; }
        }
    }

    public class HttpWosRetrieveMetadataResponse
    {
        private string xDdnStatus;
        private string xDdnOid;
        private int xDdnLength;

        public string XDdnStatus
        {
            get { return xDdnStatus; }
            set { xDdnStatus = value; }
        }

        public string XDdnOid
        {
            get { return xDdnOid; }
            set { xDdnOid = value; }
        }

        public int XDdnLength
        {
            get { return xDdnLength; }
            set { xDdnLength = value; }
        }
    }
}
