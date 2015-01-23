namespace Teltec.Backup.Forms
{
    partial class MainForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.contasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.amazonS3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupPlansToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 239);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(670, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contasToolStripMenuItem,
            this.backupPlansToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(670, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // contasToolStripMenuItem
            // 
            this.contasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.amazonS3ToolStripMenuItem});
            this.contasToolStripMenuItem.Name = "contasToolStripMenuItem";
            this.contasToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.contasToolStripMenuItem.Text = "Accounts";
            // 
            // amazonS3ToolStripMenuItem
            // 
            this.amazonS3ToolStripMenuItem.Name = "amazonS3ToolStripMenuItem";
            this.amazonS3ToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.amazonS3ToolStripMenuItem.Text = "Amazon S3";
            this.amazonS3ToolStripMenuItem.Click += new System.EventHandler(this.amazonS3ToolStripMenuItem_Click);
            // 
            // backupPlansToolStripMenuItem
            // 
            this.backupPlansToolStripMenuItem.Name = "backupPlansToolStripMenuItem";
            this.backupPlansToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.backupPlansToolStripMenuItem.Text = "Backup Plans";
            this.backupPlansToolStripMenuItem.Click += new System.EventHandler(this.backupPlansToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 261);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Teltec Backup";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem amazonS3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupPlansToolStripMenuItem;
    }
}

