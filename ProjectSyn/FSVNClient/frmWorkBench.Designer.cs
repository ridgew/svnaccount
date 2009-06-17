namespace FSVNClient
{
    partial class frmWorkBench
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
            this.fileMon = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.fileMon)).BeginInit();
            this.SuspendLayout();
            // 
            // fileMon
            // 
            this.fileMon.EnableRaisingEvents = true;
            this.fileMon.IncludeSubdirectories = true;
            this.fileMon.Path = global::FSVNClient.Properties.Settings.Default.WorkDirectory;
            this.fileMon.SynchronizingObject = this;
            this.fileMon.Renamed += new System.IO.RenamedEventHandler(this.fileMon_Renamed);
            this.fileMon.Deleted += new System.IO.FileSystemEventHandler(this.fileMon_Deleted);
            this.fileMon.Created += new System.IO.FileSystemEventHandler(this.fileMon_Created);
            this.fileMon.Changed += new System.IO.FileSystemEventHandler(this.fileMon_Changed);
            // 
            // frmWorkBench
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "frmWorkBench";
            this.Text = "项目资源工作台";
            ((System.ComponentModel.ISupportInitialize)(this.fileMon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.FileSystemWatcher fileMon;
    }
}