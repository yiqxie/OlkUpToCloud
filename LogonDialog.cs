using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileToUpload
{
    public partial class LogonDialog : Form
    {
        public LogonDialog()
        {
            InitializeComponent();
            tbEmailAddress.Text = Globals.ThisAddIn.Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress.Trim();
            tbPassword.Focus();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            strEmailAddress = tbEmailAddress.Text.Trim();
            strPassword = tbPassword.Text.Trim();

            if (strEmailAddress == "" || strPassword == "")
            {
                MessageBox.Show("1、如果您已是云盘用户，请输入正确的邮箱密码（开机密码）\n\r2、如果您还不是云盘用户请联系所在机构IT咨询如何申请开通，不要重复尝试登录避免邮箱帐号被锁定", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private String strEmailAddress, strPassword;

        public string EmailAddress
        {
            get { return strEmailAddress; }
            set { strEmailAddress = value; }
        }

        public string Password
        {
            get { return strPassword; }
            set { strPassword = value; }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {

        }

        private void tbPassword_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyValue == 13)
            //{ 
            //    btOK_Click(sender, new EventArgs());
            //}
        }
    }
}
