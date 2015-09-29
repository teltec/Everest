namespace Teltec.Backup.App.Forms.BackupPlan
{
	partial class BackupPlanPurgeOptionsForm
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
			this.nudNumberOfVersionsToKeep = new System.Windows.Forms.NumericUpDown();
			this.cbEnabledKeepNumberOfVersions = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonGroup1 = new Teltec.Common.Controls.RadioButtonGroup(this.components);
			this.rbtnCustom = new Teltec.Common.Controls.GroupableRadioButton();
			this.rbtnDefault = new Teltec.Common.Controls.GroupableRadioButton();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudNumberOfVersionsToKeep)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.Text = "Purge options";
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this.panel1);
			//
			// nudNumberOfVersionsToKeep
			//
			this.nudNumberOfVersionsToKeep.Location = new System.Drawing.Point(259, 52);
			this.nudNumberOfVersionsToKeep.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudNumberOfVersionsToKeep.Name = "nudNumberOfVersionsToKeep";
			this.nudNumberOfVersionsToKeep.Size = new System.Drawing.Size(97, 20);
			this.nudNumberOfVersionsToKeep.TabIndex = 6;
			this.nudNumberOfVersionsToKeep.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			//
			// cbEnabledKeepNumberOfVersions
			//
			this.cbEnabledKeepNumberOfVersions.AutoSize = true;
			this.cbEnabledKeepNumberOfVersions.Checked = true;
			this.cbEnabledKeepNumberOfVersions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbEnabledKeepNumberOfVersions.Location = new System.Drawing.Point(33, 55);
			this.cbEnabledKeepNumberOfVersions.Name = "cbEnabledKeepNumberOfVersions";
			this.cbEnabledKeepNumberOfVersions.Size = new System.Drawing.Size(220, 17);
			this.cbEnabledKeepNumberOfVersions.TabIndex = 5;
			this.cbEnabledKeepNumberOfVersions.Text = "Number of versions to keep (for each file)";
			this.cbEnabledKeepNumberOfVersions.UseVisualStyleBackColor = true;
			//
			// panel1
			//
			this.panel1.Controls.Add(this.rbtnDefault);
			this.panel1.Controls.Add(this.rbtnCustom);
			this.panel1.Controls.Add(this.cbEnabledKeepNumberOfVersions);
			this.panel1.Controls.Add(this.nudNumberOfVersionsToKeep);
			this.panel1.Location = new System.Drawing.Point(5, 20);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 93);
			this.panel1.TabIndex = 2;
			//
			// rbtnCustom
			//
			this.rbtnCustom.AutoSize = true;
			this.rbtnCustom.Location = new System.Drawing.Point(8, 32);
			this.rbtnCustom.Name = "rbtnCustom";
			this.rbtnCustom.RadioGroup = null;
			this.rbtnCustom.Size = new System.Drawing.Size(127, 17);
			this.rbtnCustom.TabIndex = 7;
			this.rbtnCustom.TabStop = true;
			this.rbtnCustom.Text = "Custom purge options";
			this.rbtnCustom.UseVisualStyleBackColor = true;
			this.rbtnCustom.CheckedChanged += new System.EventHandler(this.PurgeTypeChanged);
			//
			// rbtnDefault
			//
			this.rbtnDefault.AutoSize = true;
			this.rbtnDefault.Location = new System.Drawing.Point(8, 8);
			this.rbtnDefault.Name = "rbtnDefault";
			this.rbtnDefault.RadioGroup = null;
			this.rbtnDefault.Size = new System.Drawing.Size(79, 17);
			this.rbtnDefault.TabIndex = 8;
			this.rbtnDefault.TabStop = true;
			this.rbtnDefault.Text = "Use default";
			this.rbtnDefault.UseVisualStyleBackColor = true;
			this.rbtnDefault.CheckedChanged += new System.EventHandler(this.PurgeTypeChanged);
			//
			// BackupPlanPurgeOptionsForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "BackupPlanPurgeOptionsForm";
			this.Text = "Backup Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.nudNumberOfVersionsToKeep)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox cbEnabledKeepNumberOfVersions;
		private System.Windows.Forms.NumericUpDown nudNumberOfVersionsToKeep;
		private Common.Controls.RadioButtonGroup radioButtonGroup1;
		private Common.Controls.GroupableRadioButton rbtnCustom;
		private Common.Controls.GroupableRadioButton rbtnDefault;

	}
}
