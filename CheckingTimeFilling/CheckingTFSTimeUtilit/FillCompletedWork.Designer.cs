
namespace CheckingTFSTimeUtilit
{
    partial class FillCompletedWork
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FillCompletedWork));
            this.Icon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // Icon
            // 
            this.Icon.Icon = ((System.Drawing.Icon)(resources.GetObject("Icon.Icon")));
            this.Icon.Text = "TFS CW controlling. RMB - stop, LMB - start, MMB - stop utilit";
            this.Icon.Visible = true;
            this.Icon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Icon_MouseClick);
            // 
            // FillCompletedWork
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 26);
            this.Name = "FillCompletedWork";
            this.Text = "Fill Completed Work";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NotifyIcon Icon;
    }
}

