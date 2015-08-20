namespace Teltec.Backup.App.Forms
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
			this.restorePlansToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.synchronizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tpgBackupPlans = new System.Windows.Forms.TabPage();
			this.backupPlanListControl1 = new Teltec.Backup.App.Forms.BackupPlan.BackupPlanListControl();
			this.tpgRestorePlans = new System.Windows.Forms.TabPage();
			this.restorePlanListControl1 = new Teltec.Backup.App.Forms.RestorePlan.RestorePlanListControl();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tpgBackupPlans.SuspendLayout();
			this.tpgRestorePlans.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 340);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(670, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contasToolStripMenuItem,
            this.backupPlansToolStripMenuItem,
            this.restorePlansToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
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
			// restorePlansToolStripMenuItem
			// 
			this.restorePlansToolStripMenuItem.Name = "restorePlansToolStripMenuItem";
			this.restorePlansToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
			this.restorePlansToolStripMenuItem.Text = "Restore Plans";
			this.restorePlansToolStripMenuItem.Click += new System.EventHandler(this.restorePlansToolStripMenuItem_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.synchronizeToolStripMenuItem,
            this.settingsToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
			this.optionsToolStripMenuItem.Text = "Options";
			// 
			// synchronizeToolStripMenuItem
			// 
			this.synchronizeToolStripMenuItem.Name = "synchronizeToolStripMenuItem";
			this.synchronizeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.synchronizeToolStripMenuItem.Text = "Synchronize";
			this.synchronizeToolStripMenuItem.Click += new System.EventHandler(this.synchronizeToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tpgBackupPlans);
			this.tabControl1.Controls.Add(this.tpgRestorePlans);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 24);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(670, 316);
			this.tabControl1.TabIndex = 3;
			this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
			// 
			// tpgBackupPlans
			// 
			this.tpgBackupPlans.Controls.Add(this.backupPlanListControl1);
			this.tpgBackupPlans.Location = new System.Drawing.Point(4, 22);
			this.tpgBackupPlans.Name = "tpgBackupPlans";
			this.tpgBackupPlans.Padding = new System.Windows.Forms.Padding(3);
			this.tpgBackupPlans.Size = new System.Drawing.Size(662, 290);
			this.tpgBackupPlans.TabIndex = 0;
			this.tpgBackupPlans.Text = "Backup";
			this.tpgBackupPlans.UseVisualStyleBackColor = true;
			// 
			// backupPlanListControl1
			// 
			this.backupPlanListControl1.AutoSize = true;
			this.backupPlanListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.backupPlanListControl1.Location = new System.Drawing.Point(3, 3);
			this.backupPlanListControl1.Name = "backupPlanListControl1";
			this.backupPlanListControl1.Size = new System.Drawing.Size(656, 284);
			this.backupPlanListControl1.TabIndex = 3;
			// 
			// tpgRestorePlans
			// 
			this.tpgRestorePlans.Controls.Add(this.restorePlanListControl1);
			this.tpgRestorePlans.Location = new System.Drawing.Point(4, 22);
			this.tpgRestorePlans.Name = "tpgRestorePlans";
			this.tpgRestorePlans.Padding = new System.Windows.Forms.Padding(3);
			this.tpgRestorePlans.Size = new System.Drawing.Size(662, 290);
			this.tpgRestorePlans.TabIndex = 1;
			this.tpgRestorePlans.Text = "Restore";
			this.tpgRestorePlans.UseVisualStyleBackColor = true;
			// 
			// restorePlanListControl1
			// 
			this.restorePlanListControl1.AutoSize = true;
			this.restorePlanListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restorePlanListControl1.Location = new System.Drawing.Point(3, 3);
			this.restorePlanListControl1.Name = "restorePlanListControl1";
			this.restorePlanListControl1.Size = new System.Drawing.Size(656, 284);
			this.restorePlanListControl1.TabIndex = 0;
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.settingsToolStripMenuItem.Text = "Settings";
			this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(670, 362);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Teltec Backup";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tpgBackupPlans.ResumeLayout(false);
			this.tpgBackupPlans.PerformLayout();
			this.tpgRestorePlans.ResumeLayout(false);
			this.tpgRestorePlans.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem amazonS3ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem backupPlansToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem restorePlansToolStripMenuItem;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tpgBackupPlans;
		private BackupPlan.BackupPlanListControl backupPlanListControl1;
		private System.Windows.Forms.TabPage tpgRestorePlans;
		private RestorePlan.RestorePlanListControl restorePlanListControl1;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem synchronizeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    }
}

