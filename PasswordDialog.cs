using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AttachmentToWos
{
    public partial class PasswordDialog : Form
    {
        private String strPassword = String.Empty;
        private int iExpire = 0;
        private Boolean bApplyAll = false;

        public string Password
        {
            get { return strPassword; }
            set { strPassword = value; }
        }

        public int Expire
        {
            get { return iExpire; }
            set { iExpire = value; }
        }

        public Boolean ApplyAll
        {
            get { return bApplyAll; }
            set { bApplyAll = value; }
        }

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void tbExpire_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))  
            {  
                e.Handled = true;  
            }  
        }

        private void btConfirm_Click(object sender, EventArgs e)
        {
            Password = tbPassword.Text.Trim();
            iExpire = Convert.ToInt32(tbExpire.Text.Trim() != "" ? tbExpire.Text.Trim() : "0");
            bApplyAll = cbApplyAll.Checked;
        }
    }
}
