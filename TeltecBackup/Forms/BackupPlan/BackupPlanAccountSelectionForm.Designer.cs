namespace Teltec.Backup.Forms.BackupPlan
{
    partial class BackupPlanAccountSelectionForm
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
			this.flpSupportedAccounts = new System.Windows.Forms.FlowLayoutPanel();
			this.panelItemAmazonS3 = new System.Windows.Forms.Panel();
			this.cbAccountsAmazonS3 = new System.Windows.Forms.ComboBox();
			this.rdbtAmazonS3 = new Teltec.Common.Forms.GroupableRadioButton();
			this.radioButtonGroupSupportedAccounts = new Teltec.Common.Forms.RadioButtonGroup(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.cbFileSystem = new System.Windows.Forms.ComboBox();
			this.rdbtnFileSystem = new Teltec.Common.Forms.GroupableRadioButton();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flpSupportedAccounts.SuspendLayout();
			this.panelItemAmazonS3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelBottom
			// 
			this.panelBottom.Location = new System.Drawing.Point(0, 372);
			this.panelBottom.Size = new System.Drawing.Size(488, 40);
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(172, 13);
			this.label1.Text = "Select your Cloud Storage account";
			// 
			// panelTop
			// 
			this.panelTop.Size = new System.Drawing.Size(488, 75);
			// 
			// panelMiddle
			// 
			this.panelMiddle.Size = new System.Drawing.Size(488, 297);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.flpSupportedAccounts);
			this.groupBox1.Size = new System.Drawing.Size(448, 257);
			// 
			// flpSupportedAccounts
			// 
			this.flpSupportedAccounts.AutoSize = true;
			this.flpSupportedAccounts.Controls.Add(this.panelItemAmazonS3);
			this.flpSupportedAccounts.Controls.Add(this.panel1);
			this.flpSupportedAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpSupportedAccounts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flpSupportedAccounts.Location = new System.Drawing.Point(5, 18);
			this.flpSupportedAccounts.Margin = new System.Windows.Forms.Padding(0);
			this.flpSupportedAccounts.Name = "flpSupportedAccounts";
			this.flpSupportedAccounts.Size = new System.Drawing.Size(438, 234);
			this.flpSupportedAccounts.TabIndex = 1;
			// 
			// panelItemAmazonS3
			// 
			this.panelItemAmazonS3.Controls.Add(this.cbAccountsAmazonS3);
			this.panelItemAmazonS3.Controls.Add(this.rdbtAmazonS3);
			this.panelItemAmazonS3.Location = new System.Drawing.Point(0, 0);
			this.panelItemAmazonS3.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemAmazonS3.Name = "panelItemAmazonS3";
			this.panelItemAmazonS3.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemAmazonS3.Size = new System.Drawing.Size(409, 34);
			this.panelItemAmazonS3.TabIndex = 0;
			// 
			// cbAccountsAmazonS3
			// 
			this.cbAccountsAmazonS3.FormattingEnabled = true;
			this.cbAccountsAmazonS3.Location = new System.Drawing.Point(152, 7);
			this.cbAccountsAmazonS3.Name = "cbAccountsAmazonS3";
			this.cbAccountsAmazonS3.Size = new System.Drawing.Size(249, 21);
			this.cbAccountsAmazonS3.TabIndex = 1;
			// 
			// rdbtAmazonS3
			// 
			this.rdbtAmazonS3.AutoSize = true;
			this.rdbtAmazonS3.Checked = true;
			this.rdbtAmazonS3.Location = new System.Drawing.Point(8, 8);
			this.rdbtAmazonS3.Name = "rdbtAmazonS3";
			this.rdbtAmazonS3.RadioGroup = this.radioButtonGroupSupportedAccounts;
			this.rdbtAmazonS3.Size = new System.Drawing.Size(79, 17);
			this.rdbtAmazonS3.TabIndex = 0;
			this.rdbtAmazonS3.TabStop = true;
			this.rdbtAmazonS3.Text = "Amazon S3";
			this.rdbtAmazonS3.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cbFileSystem);
			this.panel1.Controls.Add(this.rdbtnFileSystem);
			this.panel1.Location = new System.Drawing.Point(0, 34);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 34);
			this.panel1.TabIndex = 1;
			// 
			// cbFileSystem
			// 
			this.cbFileSystem.FormattingEnabled = true;
			this.cbFileSystem.Location = new System.Drawing.Point(152, 7);
			this.cbFileSystem.Name = "cbFileSystem";
			this.cbFileSystem.Size = new System.Drawing.Size(249, 21);
			this.cbFileSystem.TabIndex = 1;
			// 
			// rdbtnFileSystem
			// 
			this.rdbtnFileSystem.AutoSize = true;
			this.rdbtnFileSystem.Location = new System.Drawing.Point(8, 8);
			this.rdbtnFileSystem.Name = "rdbtnFileSystem";
			this.rdbtnFileSystem.RadioGroup = this.radioButtonGroupSupportedAccounts;
			this.rdbtnFileSystem.Size = new System.Drawing.Size(78, 17);
			this.rdbtnFileSystem.TabIndex = 0;
			this.rdbtnFileSystem.TabStop = true;
			this.rdbtnFileSystem.Text = "File System";
			this.rdbtnFileSystem.UseVisualStyleBackColor = true;
			// 
			// BackupPlanAccountSelectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(663, 412);
			this.Name = "BackupPlanAccountSelectionForm";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.flpSupportedAccounts.ResumeLayout(false);
			this.panelItemAmazonS3.ResumeLayout(false);
			this.panelItemAmazonS3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.FlowLayoutPanel flpSupportedAccounts;
        protected System.Windows.Forms.Panel panelItemAmazonS3;
        protected System.Windows.Forms.ComboBox cbAccountsAmazonS3;
		protected Teltec.Common.Forms.GroupableRadioButton rdbtAmazonS3;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.ComboBox cbFileSystem;
		protected Teltec.Common.Forms.GroupableRadioButton rdbtnFileSystem;
		private Common.Forms.RadioButtonGroup radioButtonGroupSupportedAccounts;
    }
}
