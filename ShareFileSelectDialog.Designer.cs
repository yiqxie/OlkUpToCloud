namespace FileToUpload
{
    partial class ShareFileSelectDialog
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
            this.lvFilelist = new System.Windows.Forms.ListView();
            this.btSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lvFilelist
            // 
            this.lvFilelist.Dock = System.Windows.Forms.DockStyle.Top;
            this.lvFilelist.Location = new System.Drawing.Point(0, 0);
            this.lvFilelist.Name = "lvFilelist";
            this.lvFilelist.Size = new System.Drawing.Size(617, 331);
            this.lvFilelist.TabIndex = 0;
            this.lvFilelist.UseCompatibleStateImageBehavior = false;
            this.lvFilelist.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvFilelist_MouseDoubleClick);
            // 
            // btSelect
            // 
            this.btSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btSelect.Location = new System.Drawing.Point(530, 337);
            this.btSelect.Name = "btSelect";
            this.btSelect.Size = new System.Drawing.Size(75, 23);
            this.btSelect.TabIndex = 2;
            this.btSelect.Text = "选择文件";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // ShareFileSelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 363);
            this.Controls.Add(this.btSelect);
            this.Controls.Add(this.lvFilelist);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShareFileSelectDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "选择网盘文件";
            this.Load += new System.EventHandler(this.ShareFileSelectDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvFilelist;
        private System.Windows.Forms.Button btSelect;
    }
}