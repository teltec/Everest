namespace Teltec.Backup.App.Forms.BackupPlan
{
    partial class BackupPlanSelectAccountForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
			this.cbAmazonS3 = new System.Windows.Forms.ComboBox();
			this.rbtnAmazonS3 = new Teltec.Common.Controls.GroupableRadioButton();
			this.radioButtonGroupSupportedAccounts = new Teltec.Common.Controls.RadioButtonGroup(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.cbFileSystem = new System.Windows.Forms.ComboBox();
			this.rbtnFileSystem = new Teltec.Common.Controls.GroupableRadioButton();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flpSupportedAccounts.SuspendLayout();
			this.panelItemAmazonS3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(172, 13);
			this.label1.Text = "Select your Cloud Storage account";
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
			this.panelItemAmazonS3.Controls.Add(this.cbAmazonS3);
			this.panelItemAmazonS3.Controls.Add(this.rbtnAmazonS3);
			this.panelItemAmazonS3.Location = new System.Drawing.Point(0, 0);
			this.panelItemAmazonS3.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemAmazonS3.Name = "panelItemAmazonS3";
			this.panelItemAmazonS3.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemAmazonS3.Size = new System.Drawing.Size(409, 34);
			this.panelItemAmazonS3.TabIndex = 0;
			// 
			// cbAmazonS3
			// 
			this.cbAmazonS3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAmazonS3.FormattingEnabled = true;
			this.cbAmazonS3.Location = new System.Drawing.Point(152, 7);
			this.cbAmazonS3.Name = "cbAmazonS3";
			this.cbAmazonS3.Size = new System.Drawing.Size(249, 21);
			this.cbAmazonS3.TabIndex = 1;
			this.cbAmazonS3.DropDown += new System.EventHandler(this.cbAmazonS3_DropDown);
			this.cbAmazonS3.SelectionChangeCommitted += new System.EventHandler(this.cbAmazonS3_SelectionChangeCommitted);
			// 
			// rbtnAmazonS3
			// 
			this.rbtnAmazonS3.AutoSize = true;
			this.rbtnAmazonS3.Checked = true;
			this.rbtnAmazonS3.Location = new System.Drawing.Point(8, 8);
			this.rbtnAmazonS3.Name = "rbtnAmazonS3";
			this.rbtnAmazonS3.RadioGroup = this.radioButtonGroupSupportedAccounts;
			this.rbtnAmazonS3.Size = new System.Drawing.Size(79, 17);
			this.rbtnAmazonS3.TabIndex = 0;
			this.rbtnAmazonS3.TabStop = true;
			this.rbtnAmazonS3.Text = "Amazon S3";
			this.rbtnAmazonS3.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cbFileSystem);
			this.panel1.Controls.Add(this.rbtnFileSystem);
			this.panel1.Location = new System.Drawing.Point(0, 34);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 34);
			this.panel1.TabIndex = 1;
			this.panel1.Visible = false;
			// 
			// cbFileSystem
			// 
			this.cbFileSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbFileSystem.FormattingEnabled = true;
			this.cbFileSystem.Location = new System.Drawing.Point(152, 7);
			this.cbFileSystem.Name = "cbFileSystem";
			this.cbFileSystem.Size = new System.Drawing.Size(249, 21);
			this.cbFileSystem.TabIndex = 1;
			this.cbFileSystem.DropDown += new System.EventHandler(this.cbFileSystem_DropDown);
			this.cbFileSystem.SelectionChangeCommitted += new System.EventHandler(this.cbFileSystem_SelectionChangeCommitted);
			// 
			// rbtnFileSystem
			// 
			this.rbtnFileSystem.AutoSize = true;
			this.rbtnFileSystem.Location = new System.Drawing.Point(8, 8);
			this.rbtnFileSystem.Name = "rbtnFileSystem";
			this.rbtnFileSystem.RadioGroup = this.radioButtonGroupSupportedAccounts;
			this.rbtnFileSystem.Size = new System.Drawing.Size(78, 17);
			this.rbtnFileSystem.TabIndex = 0;
			this.rbtnFileSystem.TabStop = true;
			this.rbtnFileSystem.Text = "File System";
			this.rbtnFileSystem.UseVisualStyleBackColor = true;
			// 
			// BackupPlanAccountSelectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "BackupPlanAccountSelectionForm";
			this.Text = "Backup Plan";
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
        protected System.Windows.Forms.ComboBox cbAmazonS3;
		protected Teltec.Common.Controls.GroupableRadioButton rbtnAmazonS3;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.ComboBox cbFileSystem;
		protected Teltec.Common.Controls.GroupableRadioButton rbtnFileSystem;
		private Teltec.Common.Controls.RadioButtonGroup radioButtonGroupSupportedAccounts;
    }
}
