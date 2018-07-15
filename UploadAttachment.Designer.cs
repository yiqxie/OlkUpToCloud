namespace FileToUpload
{
    [System.ComponentModel.ToolboxItemAttribute(false)]
    partial class UploadWosAttachment : Microsoft.Office.Tools.Outlook.FormRegionBase
    {
        public UploadWosAttachment(Microsoft.Office.Interop.Outlook.FormRegion formRegion)
            : base(Globals.Factory, formRegion)
        {
            this.InitializeComponent();
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btSelectFile = new System.Windows.Forms.Button();
            this.uploadProgressBar = new System.Windows.Forms.ProgressBar();
            this.uploadInfo = new System.Windows.Forms.Label();
            this.lbInfo = new System.Windows.Forms.Label();
            this.btTest = new System.Windows.Forms.Button();
            this.btSelectShareFile = new System.Windows.Forms.Button();
            this.pbSetting = new System.Windows.Forms.PictureBox();
            this.pbLogon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbSetting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogon)).BeginInit();
            this.SuspendLayout();
            // 
            // btSelectFile
            // 
            this.btSelectFile.Location = new System.Drawing.Point(15, 19);
            this.btSelectFile.Name = "btSelectFile";
            this.btSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btSelectFile.TabIndex = 0;
            this.btSelectFile.Text = "本地文件...";
            this.btSelectFile.UseVisualStyleBackColor = true;
            this.btSelectFile.Click += new System.EventHandler(this.btSelectFile_Click);
            // 
            // uploadProgressBar
            // 
            this.uploadProgressBar.ForeColor = System.Drawing.Color.Chartreuse;
            this.uploadProgressBar.Location = new System.Drawing.Point(186, 19);
            this.uploadProgressBar.Name = "uploadProgressBar";
            this.uploadProgressBar.Size = new System.Drawing.Size(401, 23);
            this.uploadProgressBar.TabIndex = 1;
            // 
            // uploadInfo
            // 
            this.uploadInfo.AutoSize = true;
            this.uploadInfo.Location = new System.Drawing.Point(596, 25);
            this.uploadInfo.Name = "uploadInfo";
            this.uploadInfo.Size = new System.Drawing.Size(0, 13);
            this.uploadInfo.TabIndex = 2;
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Location = new System.Drawing.Point(698, 0);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(0, 13);
            this.lbInfo.TabIndex = 3;
            // 
            // btTest
            // 
            this.btTest.Location = new System.Drawing.Point(623, 25);
            this.btTest.Name = "btTest";
            this.btTest.Size = new System.Drawing.Size(75, 23);
            this.btTest.TabIndex = 4;
            this.btTest.Text = "test12";
            this.btTest.UseVisualStyleBackColor = true;
            this.btTest.Visible = false;
            this.btTest.Click += new System.EventHandler(this.btTest_Click);
            // 
            // btSelectShareFile
            // 
            this.btSelectShareFile.Location = new System.Drawing.Point(96, 19);
            this.btSelectShareFile.Name = "btSelectShareFile";
            this.btSelectShareFile.Size = new System.Drawing.Size(75, 23);
            this.btSelectShareFile.TabIndex = 5;
            this.btSelectShareFile.Text = "网盘文件...";
            this.btSelectShareFile.UseVisualStyleBackColor = true;
            this.btSelectShareFile.Click += new System.EventHandler(this.btSelectShareFile_Click);
            // 
            // pbSetting
            // 
            this.pbSetting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbSetting.ErrorImage = null;
            this.pbSetting.Location = new System.Drawing.Point(836, 19);
            this.pbSetting.Name = "pbSetting";
            this.pbSetting.Size = new System.Drawing.Size(25, 25);
            this.pbSetting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSetting.TabIndex = 6;
            this.pbSetting.TabStop = false;
            this.pbSetting.Click += new System.EventHandler(this.pbSetting_Click);
            // 
            // pbLogon
            // 
            this.pbLogon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbLogon.ErrorImage = null;
            this.pbLogon.Location = new System.Drawing.Point(805, 19);
            this.pbLogon.Name = "pbLogon";
            this.pbLogon.Size = new System.Drawing.Size(25, 25);
            this.pbLogon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLogon.TabIndex = 7;
            this.pbLogon.TabStop = false;
            this.pbLogon.Visible = false;
            this.pbLogon.Click += new System.EventHandler(this.pbLogon_Click);
            // 
            // UploadWosAttachment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbLogon);
            this.Controls.Add(this.pbSetting);
            this.Controls.Add(this.btSelectShareFile);
            this.Controls.Add(this.btTest);
            this.Controls.Add(this.lbInfo);
            this.Controls.Add(this.uploadInfo);
            this.Controls.Add(this.uploadProgressBar);
            this.Controls.Add(this.btSelectFile);
            this.Name = "UploadWosAttachment";
            this.Size = new System.Drawing.Size(876, 52);
            this.FormRegionShowing += new System.EventHandler(this.uploadWosAttachment_FormRegionShowing);
            this.FormRegionClosed += new System.EventHandler(this.uploadWosAttachment_FormRegionClosed);
            this.ClientSizeChanged += new System.EventHandler(this.UploadWosAttachment_ClientSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pbSetting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Form Region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private static void InitializeManifest(Microsoft.Office.Tools.Outlook.FormRegionManifest manifest, Microsoft.Office.Tools.Outlook.Factory factory)
        {
            manifest.FormRegionName = "平安邮件大附件上传";
            manifest.FormRegionType = Microsoft.Office.Tools.Outlook.FormRegionType.Adjoining;
            manifest.ShowInspectorRead = false;
            manifest.ShowReadingPane = false;

        }

        #endregion

        private System.Windows.Forms.Button btSelectFile;
        private System.Windows.Forms.ProgressBar uploadProgressBar;
        private System.Windows.Forms.Label uploadInfo;
        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.Button btTest;
        private System.Windows.Forms.Button btSelectShareFile;
        private System.Windows.Forms.PictureBox pbSetting;
        private System.Windows.Forms.PictureBox pbLogon;

        public partial class uploadWosAttachmentFactory : Microsoft.Office.Tools.Outlook.IFormRegionFactory
        {
            public event Microsoft.Office.Tools.Outlook.FormRegionInitializingEventHandler FormRegionInitializing;

            private Microsoft.Office.Tools.Outlook.FormRegionManifest _Manifest;

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public uploadWosAttachmentFactory()
            {
                this._Manifest = Globals.Factory.CreateFormRegionManifest();
                UploadWosAttachment.InitializeManifest(this._Manifest, Globals.Factory);
                this.FormRegionInitializing += new Microsoft.Office.Tools.Outlook.FormRegionInitializingEventHandler(this.uploadWosAttachmentFactory_FormRegionInitializing);
            }

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public Microsoft.Office.Tools.Outlook.FormRegionManifest Manifest
            {
                get
                {
                    return this._Manifest;
                }
            }

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            Microsoft.Office.Tools.Outlook.IFormRegion Microsoft.Office.Tools.Outlook.IFormRegionFactory.CreateFormRegion(Microsoft.Office.Interop.Outlook.FormRegion formRegion)
            {
                UploadWosAttachment form = new UploadWosAttachment(formRegion);
                form.Factory = this;
                return form;
            }

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            byte[] Microsoft.Office.Tools.Outlook.IFormRegionFactory.GetFormRegionStorage(object outlookItem, Microsoft.Office.Interop.Outlook.OlFormRegionMode formRegionMode, Microsoft.Office.Interop.Outlook.OlFormRegionSize formRegionSize)
            {
                throw new System.NotSupportedException();
            }

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            bool Microsoft.Office.Tools.Outlook.IFormRegionFactory.IsDisplayedForItem(object outlookItem, Microsoft.Office.Interop.Outlook.OlFormRegionMode formRegionMode, Microsoft.Office.Interop.Outlook.OlFormRegionSize formRegionSize)
            {
                if (this.FormRegionInitializing != null)
                {
                    Microsoft.Office.Tools.Outlook.FormRegionInitializingEventArgs cancelArgs = Globals.Factory.CreateFormRegionInitializingEventArgs(outlookItem, formRegionMode, formRegionSize, false);
                    this.FormRegionInitializing(this, cancelArgs);
                    return !cancelArgs.Cancel;
                }
                else
                {
                    return true;
                }
            }

            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            Microsoft.Office.Tools.Outlook.FormRegionKindConstants Microsoft.Office.Tools.Outlook.IFormRegionFactory.Kind
            {
                get
                {
                    return Microsoft.Office.Tools.Outlook.FormRegionKindConstants.WindowsForms;
                }
            }
        }
    }

    partial class WindowFormRegionCollection
    {
        internal UploadWosAttachment uploadWosAttachment
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.GetType() == typeof(UploadWosAttachment))
                        return (UploadWosAttachment)item;
                }
                return null;
            }
        }
    }
}
