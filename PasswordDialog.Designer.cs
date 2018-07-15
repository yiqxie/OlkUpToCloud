namespace AttachmentToWos
{
    partial class PasswordDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbExpire = new System.Windows.Forms.TextBox();
            this.btConfirm = new System.Windows.Forms.Button();
            this.cbApplyAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "共享秘密:";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(110, 29);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(144, 20);
            this.tbPassword.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "共享时限(天):";
            // 
            // tbExpire
            // 
            this.tbExpire.Location = new System.Drawing.Point(110, 61);
            this.tbExpire.MaxLength = 5;
            this.tbExpire.Name = "tbExpire";
            this.tbExpire.Size = new System.Drawing.Size(144, 20);
            this.tbExpire.TabIndex = 3;
            this.tbExpire.Text = "0";
            this.tbExpire.UseWaitCursor = true;
            this.tbExpire.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbExpire_KeyPress);
            // 
            // btConfirm
            // 
            this.btConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btConfirm.Location = new System.Drawing.Point(179, 91);
            this.btConfirm.Name = "btConfirm";
            this.btConfirm.Size = new System.Drawing.Size(75, 23);
            this.btConfirm.TabIndex = 4;
            this.btConfirm.Text = "确定";
            this.btConfirm.UseVisualStyleBackColor = true;
            this.btConfirm.Click += new System.EventHandler(this.btConfirm_Click);
            // 
            // cbApplyAll
            // 
            this.cbApplyAll.AutoSize = true;
            this.cbApplyAll.Checked = true;
            this.cbApplyAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbApplyAll.Location = new System.Drawing.Point(31, 97);
            this.cbApplyAll.Name = "cbApplyAll";
            this.cbApplyAll.Size = new System.Drawing.Size(74, 17);
            this.cbApplyAll.TabIndex = 5;
            this.cbApplyAll.Text = "应用全部";
            this.cbApplyAll.UseVisualStyleBackColor = true;
            // 
            // PasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 134);
            this.Controls.Add(this.cbApplyAll);
            this.Controls.Add(this.btConfirm);
            this.Controls.Add(this.tbExpire);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasswordDialog";
            this.Text = "PasswordDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbExpire;
        private System.Windows.Forms.Button btConfirm;
        private System.Windows.Forms.CheckBox cbApplyAll;
    }
}