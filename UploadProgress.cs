using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WebClient;

namespace AttachmentToWos
{
    public partial class UploadProgress : Form
    {

        public UploadProgress()
        {
            InitializeComponent();
        }

        public HttpWosPutResponse ExecuteWosPutStream(String strFilePath, String strFileName)
        {
            HttpWosClient httpClient = new HttpWosClient();

            try
            {
                httpClient.AttachFile(strFilePath, strFileName);

                UploadProgress upWind = new UploadProgress();

                httpClient.Url = "http://10.20.13.107/cmd/put";

                HttpWosPutResponse putRsp = httpClient.ExecuteWosPutStream(progressBar1);

                return putRsp;
            }
            catch /*Exception exp*/
            {
                return null;
            }
        
        }
        
    }
}
