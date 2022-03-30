
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
            this.IntervalChecking = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // FillCompletedWork
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 274);
            this.Name = "FillCompletedWork";
            this.Text = "Fill Completed Work";
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker IntervalChecking;
    }
}

