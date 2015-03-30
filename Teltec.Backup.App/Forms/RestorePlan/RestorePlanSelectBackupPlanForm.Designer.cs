namespace Teltec.Backup.App.Forms.RestorePlan
{
    partial class RestorePlanSelectBackupPlanForm
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
			this.flpSupportedAccounts = new System.Windows.Forms.FlowLayoutPanel();
			this.panelItemBackupPlans = new System.Windows.Forms.Panel();
			this.cbBackupPlan = new System.Windows.Forms.ComboBox();
			this.lblBackupPlan = new System.Windows.Forms.Label();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flpSupportedAccounts.SuspendLayout();
			this.panelItemBackupPlans.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(343, 13);
			this.label1.Text = "Select the Backup Plan which contains the backup you want to restore";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.flpSupportedAccounts);
			// 
			// flpSupportedAccounts
			// 
			this.flpSupportedAccounts.AutoSize = true;
			this.flpSupportedAccounts.Controls.Add(this.panelItemBackupPlans);
			this.flpSupportedAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpSupportedAccounts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flpSupportedAccounts.Location = new System.Drawing.Point(5, 18);
			this.flpSupportedAccounts.Margin = new System.Windows.Forms.Padding(0);
			this.flpSupportedAccounts.Name = "flpSupportedAccounts";
			this.flpSupportedAccounts.Size = new System.Drawing.Size(409, 283);
			this.flpSupportedAccounts.TabIndex = 1;
			// 
			// panelItemBackupPlans
			// 
			this.panelItemBackupPlans.Controls.Add(this.lblBackupPlan);
			this.panelItemBackupPlans.Controls.Add(this.cbBackupPlan);
			this.panelItemBackupPlans.Location = new System.Drawing.Point(0, 0);
			this.panelItemBackupPlans.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemBackupPlans.Name = "panelItemBackupPlans";
			this.panelItemBackupPlans.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemBackupPlans.Size = new System.Drawing.Size(409, 34);
			this.panelItemBackupPlans.TabIndex = 0;
			// 
			// cbBackupPlan
			// 
			this.cbBackupPlan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbBackupPlan.FormattingEnabled = true;
			this.cbBackupPlan.Location = new System.Drawing.Point(152, 7);
			this.cbBackupPlan.Name = "cbBackupPlan";
			this.cbBackupPlan.Size = new System.Drawing.Size(249, 21);
			this.cbBackupPlan.TabIndex = 1;
			this.cbBackupPlan.DropDown += new System.EventHandler(this.cbBackupPlan_DropDown);
			this.cbBackupPlan.SelectionChangeCommitted += new System.EventHandler(this.cbBackupPlan_SelectionChangeCommitted);
			// 
			// lblBackupPlan
			// 
			this.lblBackupPlan.AutoSize = true;
			this.lblBackupPlan.Location = new System.Drawing.Point(8, 10);
			this.lblBackupPlan.Name = "lblBackupPlan";
			this.lblBackupPlan.Size = new System.Drawing.Size(68, 13);
			this.lblBackupPlan.TabIndex = 2;
			this.lblBackupPlan.Text = "Backup Plan";
			// 
			// RestorePlanSelectBackupPlanForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "RestorePlanSelectBackupPlanForm";
			this.Text = "Restore Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.flpSupportedAccounts.ResumeLayout(false);
			this.panelItemBackupPlans.ResumeLayout(false);
			this.panelItemBackupPlans.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.FlowLayoutPanel flpSupportedAccounts;
        protected System.Windows.Forms.Panel panelItemBackupPlans;
		protected System.Windows.Forms.ComboBox cbBackupPlan;
		private System.Windows.Forms.Label lblBackupPlan;
    }
}
