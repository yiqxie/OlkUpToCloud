using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace FileToUpload
{
    public enum LogLevel { None = 0, Error, Warning, Information, Verbose}

    class LogTrace
    {
        private static string logPath = System.Environment.GetEnvironmentVariable("TEMP");
                
        /// 保存日志的文件夹     
        /// </summary>      
        public static string LogPath     
        {          
            get         
            {              
                if (logPath == string.Empty)             
                {
                    logPath = System.Environment.GetEnvironmentVariable("TEMP");                
                }
                return logPath;
            }
            set { logPath = value; }
        }

        private static string logFielPrefix = "OLK_ATC";     
        /// <summary>     
        /// 日志文件前缀     
        /// </summary> 
        public static string LogFielPrefix
        {
            get { return logFielPrefix; }
            set { logFielPrefix = value; }
        }

        private static string logFie = "Tracing";

        /// <summary>     
        /// 日志文件前缀     
        /// </summary> 
        public static string LogFie
        {
            get { return logFie; }
            set { logFie = value; }
        }

        /// <summary>     
        /// 写日志     
        /// </summary> 
        private static void WriteLog(LogLevel logLevel, string msg)
        {
            try
            {
                String strLogFile = LogPath + "\\" + LogFielPrefix + "_" + LogFie + "_" + DateTime.Now.ToString("yyyyMMddHH") + ".Log";

                if (File.Exists(strLogFile))
                {
                    System.IO.StreamWriter sw = System.IO.File.AppendText(strLogFile);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + logLevel.ToString() + "    " + msg);
                    sw.Close();
                }
                else
                {
                    System.IO.StreamWriter sw = File.CreateText(strLogFile);//System.IO.File.AppendText(strLogFile);
                    sw.WriteLine("DateTime  LogLevel    Message");
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + logLevel.ToString() + "    " + msg);
                    sw.Close();
                }

                
            }
            catch { }
        }

        private void TraceLog(LogLevel logLevel, string format, params Object[] args)
        {
            String strMsg = string.Format(format, args);
            try
            {
                LogLevel lLevel = LogLevel.Information;
                try
                {
                    RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\14.0\Outlook\Addins\FileToUpload", true);

                    lLevel = (LogLevel)Enum.Parse(typeof(LogLevel), regKey.GetValue("logLevel").ToString());
                }
                catch {
                    lLevel = LogLevel.Information;
                }

                if (logLevel <= lLevel)
                {
                    WriteLog(logLevel, strMsg);
                }
 
            }
            catch (Exception ep)
            {
                WriteLog(LogLevel.Error, string.Format("Read profile failed with: {0}",ep.Message));
            } 
        }

        public void TraceInfo(String format, params Object[] args)
        {
            TraceLog(LogLevel.Information, format, args);
        }

        public void TraceWarning(String format, params Object[] args)
        {
            TraceLog(LogLevel.Warning, format, args);
        }

        public void TraceError(String format, params Object[] args)
        {
            TraceLog(LogLevel.Error, format, args);
        }

        public void TraceVerbose(String format, params Object[] args)
        {
            TraceLog(LogLevel.Verbose, format, args);
        }

        public void TraceException(Exception ept)
        {
            TraceLog(LogLevel.Error, "Exception with Information: {0}", ept.Message);
            TraceLog(LogLevel.Error, "Exception in stack: {0}", ept.StackTrace);
        }
    }
    
}
