using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using System.IO;
using WebClient;
using System.Threading;

namespace AttachmentToWos
{
    public partial class WosAttachment
    {
        private void WosAttachment_Load(object sender, RibbonUIEventArgs e)
        {

        }

        public void ThreadCallback()
        {
            String strFilePath = null;
            String strFileName = null;
            OpenFileDialog slAttachFile = new OpenFileDialog();
            if (slAttachFile.ShowDialog() == DialogResult.OK)
            {
                strFilePath = slAttachFile.FileName;
                strFileName = slAttachFile.SafeFileName;
            }

            UploadProgress upWind = new UploadProgress();

            upWind.Show();

            HttpWosPutResponse putRsp = upWind.ExecuteWosPutStream(strFilePath, strFileName);

            upWind.Close();
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            String strFilePath = null;
            String strFileName = null;
            OpenFileDialog slAttachFile = new OpenFileDialog();
            if (slAttachFile.ShowDialog() == DialogResult.OK)
            {
                strFilePath = slAttachFile.FileName;
                strFileName = slAttachFile.SafeFileName;
            }

            //MessageBox.Show(strFilePath + "  " + strFileName);

            Outlook.Application objApplication = Globals.ThisAddIn.Application;
            Outlook.Inspector objInspector = objApplication.ActiveInspector();
            Outlook.MailItem objMailItem = objInspector.CurrentItem;

            /*if (objMailItem != null)
            {
                MessageBox.Show("Find mail item");
            }*/

            //HttpWosClient httpClient = new HttpWosClient();

            //httpClient.Url = "http://10.20.13.107/cmd/reserve";

            //HttpWosReserveResponse reserveRsp = httpClient.ExecuteWosReserveRequest();

            //MessageBox.Show("Reserve OID: " + reserveRsp.XDdnOid);


            try
            {
                //httpClient.AttachFile(strFilePath, strFileName);

                UploadProgress upWind = new UploadProgress();

                upWind.Show();

                HttpWosPutResponse putRsp = upWind.ExecuteWosPutStream(strFilePath, strFileName);

                upWind.Close();

                //httpClient.Url = "http://10.20.13.107/cmd/put";

                //HttpWosPutResponse putRsp = httpClient.ExecuteWosPutStream(pb);

                //upWind.Close();
                //MessageBox.Show("Upload file status " + putRsp.XDdnStatus);
                objMailItem.Body = objMailItem.Body + putRsp.XDdnOid + "\r\n";

                /*httpClient.AttachFile(strFilePath, strFileName);

                httpClient.Url = "http://10.20.13.107/cmd/putoid";

                HttpWosPutOidResponse putOidRsp = httpClient.ExecuteWosPutOidRequest(reserveRsp.XDdnOid);

                MessageBox.Show("Upload file status " + putOidRsp.XDdnStatus);

                objMailItem.Body = objMailItem.Body + putOidRsp.XDdnOid + "\r\n";*/

            }
            catch(Exception ept)
            {
                MessageBox.Show(ept.Message);
                MessageBox.Show(ept.StackTrace);
            }

            

            try
            {
                objMailItem.Attachments.Add(strFilePath, Outlook.OlAttachmentType.olByValue, 1, strFileName);
                objMailItem.Save();
            }
            catch (Exception ept)
            {
                MessageBox.Show("add attachment failed.  " + ept.Message);
            }

        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            Thread.Sleep(60000);
        }
    }
}
