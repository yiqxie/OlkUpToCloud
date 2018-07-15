namespace AttachmentToWos
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
            this.SuspendLayout();
            // 
            // btSelectFile
            // 
            this.btSelectFile.Location = new System.Drawing.Point(24, 19);
            this.btSelectFile.Name = "btSelectFile";
            this.btSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btSelectFile.TabIndex = 0;
            this.btSelectFile.Text = "选择文件...";
            this.btSelectFile.UseVisualStyleBackColor = true;
            this.btSelectFile.Click += new System.EventHandler(this.btSelectFile_Click);
            // 
            // uploadProgressBar
            // 
            this.uploadProgressBar.Location = new System.Drawing.Point(117, 19);
            this.uploadProgressBar.Name = "uploadProgressBar";
            this.uploadProgressBar.Size = new System.Drawing.Size(401, 23);
            this.uploadProgressBar.TabIndex = 1;
            // 
            // uploadInfo
            // 
            this.uploadInfo.AutoSize = true;
            this.uploadInfo.Location = new System.Drawing.Point(542, 28);
            this.uploadInfo.Name = "uploadInfo";
            this.uploadInfo.Size = new System.Drawing.Size(0, 13);
            this.uploadInfo.TabIndex = 2;
            // 
            // UploadWosAttachment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uploadInfo);
            this.Controls.Add(this.uploadProgressBar);
            this.Controls.Add(this.btSelectFile);
            this.Name = "UploadWosAttachment";
            this.Size = new System.Drawing.Size(876, 52);
            this.FormRegionShowing += new System.EventHandler(this.uploadWosAttachment_FormRegionShowing);
            this.FormRegionClosed += new System.EventHandler(this.uploadWosAttachment_FormRegionClosed);
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
            manifest.FormRegionName = "大附件上传";
            manifest.FormRegionType = Microsoft.Office.Tools.Outlook.FormRegionType.Adjoining;
            manifest.ShowInspectorRead = false;
            manifest.ShowReadingPane = false;

        }

        #endregion

        private System.Windows.Forms.Button btSelectFile;
        private System.Windows.Forms.ProgressBar uploadProgressBar;
        private System.Windows.Forms.Label uploadInfo;

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
