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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileToUpload
{
    class WosHttpClient
    {
        #region fields
        private HttpUploadingFile upfile = null;
        private string url;

        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }
        #endregion


        #region properties
        /// <summary>
        /// 获取或设置请求资源的地址
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        #endregion

        #region constructors
        /// <summary>
        /// 构造新的WosHttpClient实例
        /// </summary>
        public WosHttpClient()
            : this(null)
        {
        }

        /// <summary>
        /// 构造新的WosHttpClient实例
        /// </summary>
        /// <param name="url">要获取的资源的地址</param>
        public WosHttpClient(string url)
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
            upfile = new HttpUploadingFile(data, fileName, fieldName);
        }
        #endregion

        /// <summary>
        /// 清空Files.
        /// 在发出一个包含上述信息的请求后,必须调用此方法或手工设置相应属性以使下一次请求不会受到影响.
        /// </summary>
        public void Reset()
        {
            upfile = null;
        }

        #region WosOperation

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFileTokenRequest(String strUserName,String strPassword)
        {
            LogTrace.TraceInfo("CreateSeaFileTokenRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/auth-token/");

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);

            String strPost = "username=" + strUserName + "&password=" + strPassword;
            
            req.Method = "POST";
            req.ContentType = " application/x-www-form-urlencoded";
            req.Timeout = 10000;
            req.KeepAlive = false;

            UTF8Encoding encode = new UTF8Encoding();
            memoryStream.Write(encode.GetBytes(strPost), 0, encode.GetBytes(strPost).Length);
            writer.Flush();


            using (Stream stream = req.GetRequestStream())
            {
                memoryStream.WriteTo(stream);
            }

            memoryStream.Close();
            writer.Close();


            return req;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFileGetDefaultLibraryRequest(String strToken)
        {
            LogTrace.TraceInfo("CreateSeaFileGetDefaultLibraryRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/default-repo/");

            req.Method = "Get";
            req.ContentType = " application/x-www-form-urlencoded";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 15000;
            req.KeepAlive = false;

            return req;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFileGetUpdateLinkRequest(String strToken, String strRepoId)
        {
            LogTrace.TraceInfo("CreateSeaFileGetUpdateLinkRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/repos/" + strRepoId + "/upload-link/");

            req.Method = "Get";
            //req.ContentType = " application/x-www-form-urlencoded";
            req.Accept = "*/*";
            req.UserAgent = "curl/7.19.0 (i586-pc-mingw32msvc) libcurl/7.19.0 zlib/1.2.3";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 10000;
            req.KeepAlive = false;

            return req;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFileGetUnseenMessageRequest(String strToken)
        {
            LogTrace.TraceInfo("CreateSeaFileGetUnseenMessageRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/unseen_messages/");

            req.Method = "Get";
            req.ContentType = " application/x-www-form-urlencoded";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 10000;
            req.KeepAlive = false;

            return req;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFileGetDirectoryEntriesRequest(String strToken, String strRepoId, String strPath)
        {
            LogTrace.TraceInfo("CreateSeaFileGetDirectoryEntriesRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/repos/"+strRepoId+"/dir/?p="+strPath);

            req.Method = "Get";
            req.Accept = "application/json; indent=4";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 15000;
            req.KeepAlive = false;

            return req;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        private HttpWebRequest CreateSeaFilePingRequest(String strToken)
        {
            LogTrace.TraceInfo("CreateSeaFilePingRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/auth/ping/");

            req.Method = "Get";
            req.Accept = "application/json; indent=4";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 5000;
            req.KeepAlive = false;

            return req;
        }

        //************************************************************************************************
        //** Execution Functions
        //************************************************************************************************

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public HttpSeaFileTokenResponse ExecuteSeaFileTokenRequest(String strUser, String strPassword)
        {
            LogTrace.TraceInfo("ExecuteSeaFileTokenRequest");

            HttpSeaFileTokenResponse rsp = new HttpSeaFileTokenResponse();

            try
            {
                HttpWebRequest req = CreateSeaFileTokenRequest(strUser, strPassword);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    LogTrace.TraceInfo("Response code for TokenRequest is {0}", res.StatusCode);

                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    JObject jo = (JObject)JsonConvert.DeserializeObject(strResponse);
                    rsp.Token = jo["token"].ToString();

                    req.Abort();
                    res.Close();

                    return rsp;                  
                }

                return null;
              
            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return null;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public HttpSeaFileGetDefaultLibraryResponse ExecuteSeaFileGetDefaultLibraryRequest(String strToken)
        {
            LogTrace.TraceInfo("ExecuteSeaFileGetDefaultLibraryRequest");

            HttpSeaFileGetDefaultLibraryResponse rsp = new HttpSeaFileGetDefaultLibraryResponse();

            try
            {
                HttpWebRequest req = CreateSeaFileGetDefaultLibraryRequest(strToken);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    LogTrace.TraceInfo("Response code for GetDefaultLibrary is {0}", res.StatusCode);
                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    JObject jo = (JObject)JsonConvert.DeserializeObject(strResponse);
                    rsp.RepoId = jo["repo_id"].ToString();
                    rsp.Exists = jo["exists"].ToString();

                    req.Abort();
                    res.Close();

                    return rsp;
                }

                return null;

            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return null;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public HttpSeaFileGetUpdateLinkResponse ExecuteSeaFileGetUpdateLinkRequest(String strToken, String strRepoId)
        {
            LogTrace.TraceInfo("ExecuteSeaFileGetUpdateLinkRequest");

            HttpSeaFileGetUpdateLinkResponse rsp = new HttpSeaFileGetUpdateLinkResponse();

            try
            {
                HttpWebRequest req = CreateSeaFileGetUpdateLinkRequest(strToken,strRepoId);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    LogTrace.TraceInfo("Response code for GetUpdateLink is {0}", res.StatusCode);

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    rsp.URL = sr.ReadToEnd();

                    req.Abort();
                    res.Close();

                    return rsp;
                }

                return null;
            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return null;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="bgw">后台进程</param>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public String ExecuteBackGroundShareFileUploadFileStream(BackgroundWorker bgw, String strToken, String strURL, String strPath)
        {
            LogTrace.TraceInfo("ExecuteBackGroundShareFileUploadFileStream");

            Uri uri1 = new Uri(strURL.Replace("\"",""));
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri1);

            try
            {
                if (upfile != null)
                {
                    int iStep = GetStepSize(upfile.Data.Length);
                    String strBoundary = DateTime.Now.Ticks.ToString("X");

                    req.Method = "POST";
                    req.UserAgent = "curl/7.19.0 (i586-pc-mingw32msvc) libcurl/7.19.0 zlib/1.2.3";
                    req.Accept = "*/*";
                    req.ContentType = "multipart/form-data; boundary=----------------------------" + strBoundary;
                    req.Headers.Add("Authorization: Token " + strToken);

                    req.AllowWriteStreamBuffering = false;
                    req.Timeout = 5*60*1000;

                    UTF8Encoding encode = new UTF8Encoding();
                    //ASCIIEncoding encode = new ASCIIEncoding();

                    byte[] brPayLoad1 = encode.GetBytes("------------------------------" + strBoundary + "\r\n");
                    byte[] brPayLoad2 = encode.GetBytes("Content-Disposition: form-data; name=\"file\"; filename=\"" + upfile.FieldName + "\"\r\n");
                    byte[] brPayLoad3 = encode.GetBytes("Content-Type: text/plain\r\n\r\n");

                    byte[] brPayLoad4 = encode.GetBytes("\r\n------------------------------" + strBoundary + "\r\n");
                    byte[] brPayLoad5 = encode.GetBytes("Content-Disposition: form-data; name=\"filename\"\r\n\r\n");

                    byte[] brPayLoad6 = encode.GetBytes(upfile.FieldName + "\r\n");
                    byte[] brPayLoad7 = encode.GetBytes("HTTPPayloadLine: ------------------------------" + strBoundary + "\r\n");
                    byte[] brPayLoad8 = encode.GetBytes("Content-Disposition: form-data; name=\"parent_dir\"\r\n\r\n");

                    byte[] brPayLoad9 = encode.GetBytes(strPath + "/\r\n");
                    byte[] brPayLoad10 = encode.GetBytes("------------------------------" + strBoundary + "--");

                    req.SendChunked = true;

                    Stream postStream = req.GetRequestStream();

                    postStream.Write(brPayLoad1, 0, brPayLoad1.Length);
                    postStream.Write(brPayLoad2, 0, brPayLoad2.Length);
                    postStream.Write(brPayLoad3, 0, brPayLoad3.Length);

                    byte[] postData = upfile.ReadData(iStep);
                    int i = 0;

                    while (postData != null)
                    {
                        postStream.Write(postData, 0, postData.Length);
                        Thread.Sleep(1);
                        bgw.ReportProgress(i / iStep, upfile.FieldName);

                        i += iStep;
                        postData = upfile.ReadData(iStep);
                    }

                    bgw.ReportProgress((int)Math.Ceiling((double)upfile.Data.Length / (double)iStep), upfile.FieldName);

                    postStream.Write(brPayLoad4, 0, brPayLoad4.Length);
                    postStream.Write(brPayLoad5, 0, brPayLoad5.Length);
                    postStream.Write(brPayLoad6, 0, brPayLoad6.Length);
                    postStream.Write(brPayLoad7, 0, brPayLoad7.Length);
                    postStream.Write(brPayLoad8, 0, brPayLoad8.Length);
                    postStream.Write(brPayLoad9, 0, brPayLoad9.Length);
                    postStream.Write(brPayLoad10, 0, brPayLoad10.Length);

                    postStream.Close();
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    LogTrace.TraceInfo("Response code for UploadFile is {0}, uploaded file is {1}", res.StatusCode, upfile.FileName);

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    String strReturn = sr.ReadToEnd();

                    req.Abort();
                    res.Close();

                    return strReturn;
                }
                else
                {
                    LogTrace.TraceError("Not found upload file.");
                    return null;
                }
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return null;
            }
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        public String ExecuteCreateSeaFileCreateDownloadLinkRequest(String strToken, String strRepoId, String strPath, String strPassword, int iExpire)
        {
            LogTrace.TraceInfo("ExecuteCreateSeaFileCreateDownloadLinkRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/repos/" + strRepoId + "/file/shared-link/");

            req.Method = "PUT";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "application/json; indent=4";
            req.UserAgent = "curl/7.19.0 (i586-pc-mingw32msvc) libcurl/7.19.0 zlib/1.2.3";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 10000;
            req.KeepAlive = false;

            String strPutContent = "p=" + strPath;
            if (strPassword != null && strPassword != String.Empty)
            {
                strPutContent += "&password=" + strPassword;
            }

            if (iExpire > 0)
            {
                strPutContent += "&expire=" + iExpire.ToString();
            }

            UTF8Encoding code = new UTF8Encoding();

            try
            {
                Stream postStream = req.GetRequestStream();

                postStream.Write(code.GetBytes(strPutContent), 0, code.GetBytes(strPutContent).Length);

                postStream.Close();

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                if (res.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    LogTrace.TraceInfo("Response code for CreateDownloadLink is {0}", res.StatusCode);

                    req.Abort();
                    res.Close();

                    return res.Headers["Location"].ToString();
                }

                req.Abort();
                res.Close();
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return String.Empty;
            }

            return String.Empty;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        public Boolean ExecuteCreateSeaFileCreateNewDirectoryRequest(String strToken, String strRepoId, String strPath)
        {
            LogTrace.TraceInfo("ExecuteCreateSeaFileCreateNewDirectoryRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/repos/" + strRepoId + "/dir/?p="+strPath);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "application/json; indent=4";
            req.UserAgent = "curl/7.19.0 (i586-pc-mingw32msvc) libcurl/7.19.0 zlib/1.2.3";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 10000;
            req.KeepAlive = false;

            String strPostContent = "operation=mkdir";

            UTF8Encoding code = new UTF8Encoding();

            try
            {
                Stream postStream = req.GetRequestStream();

                postStream.Write(code.GetBytes(strPostContent), 0, code.GetBytes(strPostContent).Length);

                postStream.Close();

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                if (res.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    LogTrace.TraceInfo("Response code for CreateNewDirectory is {0}", res.StatusCode);

                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    req.Abort();
                    res.Close();

                    if (strResponse == "\"success\"")
                    {
                        return true;
                    }
                }
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return false;
            }

            return false;
        }

        /// <summary>
        /// 创建PutOid请求，通过之前预申请的oid向Wos云端上传文件
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWebRequest对象</returns>
        public String ExecuteCreateSeaFileCreateLibraryRequest(String strToken, String strLibrary)
        {
            LogTrace.TraceInfo("ExecuteCreateSeaFileCreateLibraryRequest");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "/api2/repos/");

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "application/json; indent=4";
            req.UserAgent = "curl/7.19.0 (i586-pc-mingw32msvc) libcurl/7.19.0 zlib/1.2.3";
            req.Headers.Add("Authorization: Token " + strToken);
            req.Timeout = 10000;
            req.KeepAlive = false;

            String strPostContent = "name=" + strLibrary.Trim() + "&desc=new library";

            UTF8Encoding code = new UTF8Encoding();

            try
            {
                Stream postStream = req.GetRequestStream();

                postStream.Write(code.GetBytes(strPostContent), 0, code.GetBytes(strPostContent).Length);

                postStream.Close();

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                LogTrace.TraceInfo("Response code for CreateLibrary is {0}", res.StatusCode);

                String strResponse = String.Empty;

                Stream rStream = res.GetResponseStream();
                StreamReader sr = new StreamReader(rStream);
                strResponse = sr.ReadToEnd();

                req.Abort();
                res.Close();

                LogTrace.TraceVerbose("Got Server response [{0}]", strResponse);

                JObject jo = (JObject)JsonConvert.DeserializeObject(strResponse);
                return jo["repo_id"].ToString();                   
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
                return String.Empty;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public int ExecuteSeaFileGetUnseenMessageRequest(String strToken)
        {
            LogTrace.TraceInfo("ExecuteSeaFileGetUnseenMessageRequest");

            try
            {
                HttpWebRequest req = CreateSeaFileGetUnseenMessageRequest(strToken);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    LogTrace.TraceInfo("Response code for GetUnseenMessage is {0}", res.StatusCode);

                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    req.Abort();
                    res.Close();

                    JObject jo = (JObject)JsonConvert.DeserializeObject(strResponse);
                    return Convert.ToInt32(jo["count"].ToString());
                }

                return 0;

            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return 0;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public HttpSeaFileGetDirectoryEntriesResponse ExecuteSeaFileGetDirectoryEntriesRequest(String strToken, String strRepoId, string strPath)
        {
            LogTrace.TraceInfo("ExecuteSeaFileGetDirectoryEntriesRequest");

            try
            {
                HttpSeaFileGetDirectoryEntriesResponse rsp = new HttpSeaFileGetDirectoryEntriesResponse();

                HttpWebRequest req = CreateSeaFileGetDirectoryEntriesRequest(strToken,strRepoId,strPath);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    LogTrace.TraceInfo("Response code for GetDirectoryEntries is {0}", res.StatusCode);

                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    LogTrace.TraceVerbose("Got Server response [{0}]",strResponse);

                    strResponse = strResponse.Replace("[", "{\"root\":[");
                    strResponse = strResponse.Replace("]","]}");

                    LogTrace.TraceVerbose("Got Server response replace [{0}]", strResponse);
                    
                    JObject jo = (JObject)JsonConvert.DeserializeObject(strResponse);
                    JToken token = (JToken)jo["root"].First;
                    
                    while (token != null)
                    {
                        int iSize = 0;
                        if(token["type"].ToString() == "file") 
                            iSize = Convert.ToInt32(token["size"].ToString());

                        rsp.AddEntry(
                            token["permission"].ToString(),
                            token["mtime"].ToString(),
                            token["type"].ToString(),
                            token["name"].ToString(),
                            token["id"].ToString(),
                            iSize);

                        token = (JToken)token.Next;
                    }

                    req.Abort();
                    res.Close();

                    return rsp;
                }

                return null;

            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return null;
            }
        }

        /// <summary>
        /// 发送PutOid请求,并返回获得的回应
        /// </summary>
        /// <param name="oid">Wos对象标识</param>
        /// <returns>返回HttpWosPutOidResponse对象</returns>
        public Boolean ExecuteSeaFilePingRequest(String strToken)
        {
            LogTrace.TraceInfo("ExecuteSeaFilePingRequest");

            try
            {
                HttpWebRequest req = CreateSeaFilePingRequest(strToken);
                if (req != null)
                {
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                    LogTrace.TraceInfo("Response code for Ping is {0}", res.StatusCode);

                    String strResponse = String.Empty;

                    Stream rStream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(rStream);
                    strResponse = sr.ReadToEnd();

                    req.Abort();
                    res.Close();

                    if (strResponse != "\"pong\"")
                        return false;
                    else
                        return true;
                }

                return false;

            }
            catch(Exception ept)
            {
                LogTrace.TraceException(ept);
                return false;
            }
        }
        #endregion

        /// <summary>
        /// 计算上传文件时，根据文件大小，每次发送的字节数
        /// </summary>
        /// <param name="iFileLength">文件的大小(字节)</param>
        /// <returns>每次上传文件的字节数</returns>
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

    public class HttpUploadingFile
    {
        private string fileName = String.Empty;
        private string fieldName = String.Empty;
        private int iFileLength = 0;
        private int iReadPosition = 0;
        private byte[] data;

        private LogTrace m_LogTrace = new LogTrace();

        private LogTrace LogTrace
        {
            get { return m_LogTrace; }
        }

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

        public int FileLength
        {
            get { return iFileLength; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public byte[] ReadData(int length)
        {
            if (length > (iFileLength - iReadPosition))
                length = (int)(iFileLength - iReadPosition);

            if (length <= 0)
                return null;

            using (FileStream stream = File.OpenRead(fileName)/*new FileStream(fileName, FileMode.Open)*/)
            {
                byte[] inBytes = new byte[length];
                stream.Position = iReadPosition;
                stream.Read(inBytes, 0, inBytes.Length);
                iReadPosition += length;
                return inBytes;
            }
        }

        public void reset()
        {
            iReadPosition = 0;
        }

        public HttpUploadingFile(string fileName, string fieldName)
        {
            this.fileName = fileName;
            this.fieldName = fieldName;
            try
            {
                using (FileStream stream = File.OpenRead(fileName)/*new FileStream(fileName, FileMode.Open)*/)
                {
                    iFileLength = (int)stream.Length;
                    byte[] inBytes = new byte[stream.Length];
                    stream.Read(inBytes, 0, inBytes.Length);
                    data = inBytes;
                }
            }
            catch (Exception ept)
            {
                LogTrace.TraceException(ept);
            }
        }

        public HttpUploadingFile(byte[] data, string fileName, string fieldName)
        {
            this.data = data;
            this.fileName = fileName;
            this.fieldName = fieldName;
        }
    }

    public class HttpSeaFileTokenResponse
    {
        private string strToken;

        public string Token
        {
            get { return strToken; }
            set { strToken = value; }
        }
    }

    public class HttpSeaFileGetDefaultLibraryResponse
    {
        private string strRepoId, strExists;

        public string RepoId
        {
            get { return strRepoId; }
            set { strRepoId = value; }
        }

        public string Exists
        {
            get { return strExists; }
            set { strExists = value; }
        }
    }

    public class HttpSeaFileGetUpdateLinkResponse
    {
        private string strUrl;

        public string URL
        {
            get { return strUrl; }
            set { strUrl = value; }
        }
    }

    public class HttpSeaFileGetDirectoryEntriesNode
    {
        private string strPermission,strMtime,strType,strName,strID;
        private int iSize;

        public HttpSeaFileGetDirectoryEntriesNode(String strPermission, String strMtime, String strType, String strName, String strID,int isize)
        {
            Permission = strPermission;
            Mtime = strMtime;
            Type = strType;
            Name = strName;
            ID = strID;
            Size = iSize;
        }

        public string Permission
        {
            get { return strPermission; }
            set { strPermission = value; }
        }

        public string Mtime
        {
            get { return strMtime; }
            set { strMtime = value; }
        }

        public string Type
        {
            get { return strType; }
            set { strType = value; }
        }

        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }

        public string ID
        {
            get { return strID; }
            set { strID = value; }
        }

        public int Size
        {
            get { return iSize; }
            set { iSize = value; }
        }
    }

    public class HttpSeaFileGetDirectoryEntriesResponse
    {
        private List<HttpSeaFileGetDirectoryEntriesNode> lstEntries = new List<HttpSeaFileGetDirectoryEntriesNode>();

        public Boolean AddEntry(String strPermission, String strMtime, String strType, String strName, String strID, int iSize)
        {
            try
            {
                HttpSeaFileGetDirectoryEntriesNode node = new HttpSeaFileGetDirectoryEntriesNode(strPermission, strMtime, strType, strName, strID, iSize);
                lstEntries.Add(node);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public HttpSeaFileGetDirectoryEntriesNode GetEntry(int iEntry)
        {
            if (iEntry < lstEntries.Count)
            {
                return lstEntries[iEntry];
            }
            else
            {
                return null;
            }
        }

        public int GetEntriesCount()
        {
            return lstEntries.Count;
        }

        public List<HttpSeaFileGetDirectoryEntriesNode> GetDirectoryEntries()
        { 
            List<HttpSeaFileGetDirectoryEntriesNode> lstDirectry = new List<HttpSeaFileGetDirectoryEntriesNode>();

            foreach (HttpSeaFileGetDirectoryEntriesNode node in lstEntries)
            {
                if (node.Type == "dir")
                {
                    lstDirectry.Add(node);
                }
            }

            return lstDirectry;
        }

        public List<HttpSeaFileGetDirectoryEntriesNode> GetFileEntries()
        {
            List<HttpSeaFileGetDirectoryEntriesNode> lstDirectry = new List<HttpSeaFileGetDirectoryEntriesNode>();

            foreach (HttpSeaFileGetDirectoryEntriesNode node in lstEntries)
            {
                if (node.Type == "file")
                {
                    lstDirectry.Add(node);
                }
            }

            return lstDirectry;
        }
    }

    //public class HttpSeaFileGetLibrariesNode
    //{
    //    private string strPermission, strMtime, strType, strName, strID, strOwner, strRoot, strDesc;
    //    private int iSize;
    //    Boolean bEncrypted, bVirtual;

    //    public HttpSeaFileGetLibrariesNode(String strPermission, String strMtime, String strType, String strName, String strID, String strOwner, String strRoot, String strDesc, int isize)
    //    {
    //        Permission = strPermission;
    //        Mtime = strMtime;
    //        Type = strType;
    //        Name = strName;
    //        ID = strID;
    //        Size = iSize;
    //    }

    //    public string Permission
    //    {
    //        get { return strPermission; }
    //        set { strPermission = value; }
    //    }

    //    public string Mtime
    //    {
    //        get { return strMtime; }
    //        set { strMtime = value; }
    //    }

    //    public string Type
    //    {
    //        get { return strType; }
    //        set { strType = value; }
    //    }

    //    public string Name
    //    {
    //        get { return strName; }
    //        set { strName = value; }
    //    }

    //    public string ID
    //    {
    //        get { return strID; }
    //        set { strID = value; }
    //    }

    //    public int Size
    //    {
    //        get { return iSize; }
    //        set { iSize = value; }
    //    }
    //}

    //public class HttpSeaFileGetLibrariesResponse
    //{
    //    private List<HttpSeaFileGetDirectoryEntriesNode> lstEntries = new List<HttpSeaFileGetDirectoryEntriesNode>();

    //    public Boolean AddEntry(String strPermission, String strMtime, String strType, String strName, String strID, int iSize)
    //    {
    //        try
    //        {
    //            HttpSeaFileGetDirectoryEntriesNode node = new HttpSeaFileGetDirectoryEntriesNode(strPermission, strMtime, strType, strName, strID, iSize);
    //            lstEntries.Add(node);
    //            return true;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    public HttpSeaFileGetDirectoryEntriesNode GetEntry(int iEntry)
    //    {
    //        if (iEntry < lstEntries.Count)
    //        {
    //            return lstEntries[iEntry];
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    public int GetEntriesCount()
    //    {
    //        return lstEntries.Count;
    //    }

    //    public List<HttpSeaFileGetDirectoryEntriesNode> GetDirectoryEntries()
    //    {
    //        List<HttpSeaFileGetDirectoryEntriesNode> lstDirectry = new List<HttpSeaFileGetDirectoryEntriesNode>();

    //        foreach (HttpSeaFileGetDirectoryEntriesNode node in lstEntries)
    //        {
    //            if (node.Type == "dir")
    //            {
    //                lstDirectry.Add(node);
    //            }
    //        }

    //        return lstDirectry;
    //    }

    //    public List<HttpSeaFileGetDirectoryEntriesNode> GetFileEntries()
    //    {
    //        List<HttpSeaFileGetDirectoryEntriesNode> lstDirectry = new List<HttpSeaFileGetDirectoryEntriesNode>();

    //        foreach (HttpSeaFileGetDirectoryEntriesNode node in lstEntries)
    //        {
    //            if (node.Type == "file")
    //            {
    //                lstDirectry.Add(node);
    //            }
    //        }

    //        return lstDirectry;
    //    }
    //}
}
