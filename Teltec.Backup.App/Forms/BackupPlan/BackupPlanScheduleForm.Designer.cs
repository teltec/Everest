namespace Teltec.Backup.App.Forms.BackupPlan
{
	partial class BackupPlanScheduleForm
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
			this.panelItemAmazonS3 = new System.Windows.Forms.Panel();
			this.flpSupportedAccounts = new System.Windows.Forms.FlowLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.rbtnManual = new Teltec.Common.Controls.GroupableRadioButton();
			this.radioButtonGroupSupportedScheduleTypes = new Teltec.Common.Controls.RadioButtonGroup(this.components);
			this.panel2 = new System.Windows.Forms.Panel();
			this.dtpSpecificTime = new System.Windows.Forms.DateTimePicker();
			this.dtpSpecificDate = new System.Windows.Forms.DateTimePicker();
			this.rbtnSpecific = new Teltec.Common.Controls.GroupableRadioButton();
			this.panel3 = new System.Windows.Forms.Panel();
			this.llblEditSchedule = new System.Windows.Forms.LinkLabel();
			this.rbtnRecurring = new Teltec.Common.Controls.GroupableRadioButton();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flpSupportedAccounts.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(147, 13);
			this.label1.Text = "Define the scheduling options";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.flpSupportedAccounts);
			this.groupBox1.Controls.Add(this.panelItemAmazonS3);
			// 
			// panelItemAmazonS3
			// 
			this.panelItemAmazonS3.Location = new System.Drawing.Point(5, 18);
			this.panelItemAmazonS3.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemAmazonS3.Name = "panelItemAmazonS3";
			this.panelItemAmazonS3.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemAmazonS3.Size = new System.Drawing.Size(409, 34);
			this.panelItemAmazonS3.TabIndex = 1;
			// 
			// flpSupportedAccounts
			// 
			this.flpSupportedAccounts.AutoSize = true;
			this.flpSupportedAccounts.Controls.Add(this.panel1);
			this.flpSupportedAccounts.Controls.Add(this.panel2);
			this.flpSupportedAccounts.Controls.Add(this.panel3);
			this.flpSupportedAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpSupportedAccounts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flpSupportedAccounts.Location = new System.Drawing.Point(5, 18);
			this.flpSupportedAccounts.Margin = new System.Windows.Forms.Padding(0);
			this.flpSupportedAccounts.Name = "flpSupportedAccounts";
			this.flpSupportedAccounts.Size = new System.Drawing.Size(409, 283);
			this.flpSupportedAccounts.TabIndex = 2;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.rbtnManual);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 34);
			this.panel1.TabIndex = 0;
			// 
			// rbtnManual
			// 
			this.rbtnManual.AutoSize = true;
			this.rbtnManual.Checked = true;
			this.rbtnManual.Location = new System.Drawing.Point(8, 8);
			this.rbtnManual.Name = "rbtnManual";
			this.rbtnManual.RadioGroup = this.radioButtonGroupSupportedScheduleTypes;
			this.rbtnManual.Size = new System.Drawing.Size(89, 17);
			this.rbtnManual.TabIndex = 0;
			this.rbtnManual.TabStop = true;
			this.rbtnManual.Text = "Run manually";
			this.rbtnManual.UseVisualStyleBackColor = true;
			this.rbtnManual.CheckedChanged += new System.EventHandler(this.ScheduleTypeChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.dtpSpecificTime);
			this.panel2.Controls.Add(this.dtpSpecificDate);
			this.panel2.Controls.Add(this.rbtnSpecific);
			this.panel2.Location = new System.Drawing.Point(0, 34);
			this.panel2.Margin = new System.Windows.Forms.Padding(0);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(5);
			this.panel2.Size = new System.Drawing.Size(409, 34);
			this.panel2.TabIndex = 1;
			// 
			// dtpSpecificTime
			// 
			this.dtpSpecificTime.CustomFormat = "HH:mm tt";
			this.dtpSpecificTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpSpecificTime.Location = new System.Drawing.Point(284, 6);
			this.dtpSpecificTime.Name = "dtpSpecificTime";
			this.dtpSpecificTime.ShowUpDown = true;
			this.dtpSpecificTime.Size = new System.Drawing.Size(75, 20);
			this.dtpSpecificTime.TabIndex = 2;
			// 
			// dtpSpecificDate
			// 
			this.dtpSpecificDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpSpecificDate.Location = new System.Drawing.Point(178, 6);
			this.dtpSpecificDate.Name = "dtpSpecificDate";
			this.dtpSpecificDate.Size = new System.Drawing.Size(100, 20);
			this.dtpSpecificDate.TabIndex = 1;
			this.dtpSpecificDate.Value = new System.DateTime(2015, 10, 30, 16, 17, 0, 0);
			// 
			// rbtnSpecific
			// 
			this.rbtnSpecific.AutoSize = true;
			this.rbtnSpecific.Location = new System.Drawing.Point(8, 8);
			this.rbtnSpecific.Name = "rbtnSpecific";
			this.rbtnSpecific.RadioGroup = this.radioButtonGroupSupportedScheduleTypes;
			this.rbtnSpecific.Size = new System.Drawing.Size(87, 17);
			this.rbtnSpecific.TabIndex = 0;
			this.rbtnSpecific.TabStop = true;
			this.rbtnSpecific.Text = "Specific date";
			this.rbtnSpecific.UseVisualStyleBackColor = true;
			this.rbtnSpecific.CheckedChanged += new System.EventHandler(this.ScheduleTypeChanged);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.llblEditSchedule);
			this.panel3.Controls.Add(this.rbtnRecurring);
			this.panel3.Location = new System.Drawing.Point(0, 68);
			this.panel3.Margin = new System.Windows.Forms.Padding(0);
			this.panel3.Name = "panel3";
			this.panel3.Padding = new System.Windows.Forms.Padding(5);
			this.panel3.Size = new System.Drawing.Size(409, 34);
			this.panel3.TabIndex = 2;
			// 
			// llblEditSchedule
			// 
			this.llblEditSchedule.AutoSize = true;
			this.llblEditSchedule.LinkArea = new System.Windows.Forms.LinkArea(1, 13);
			this.llblEditSchedule.Location = new System.Drawing.Point(178, 8);
			this.llblEditSchedule.Name = "llblEditSchedule";
			this.llblEditSchedule.Size = new System.Drawing.Size(78, 17);
			this.llblEditSchedule.TabIndex = 1;
			this.llblEditSchedule.TabStop = true;
			this.llblEditSchedule.Text = "(edit schedule)";
			this.llblEditSchedule.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.llblEditSchedule.UseCompatibleTextRendering = true;
			this.llblEditSchedule.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblEditSchedule_LinkClicked);
			// 
			// rbtnRecurring
			// 
			this.rbtnRecurring.AutoSize = true;
			this.rbtnRecurring.Location = new System.Drawing.Point(8, 8);
			this.rbtnRecurring.Name = "rbtnRecurring";
			this.rbtnRecurring.RadioGroup = this.radioButtonGroupSupportedScheduleTypes;
			this.rbtnRecurring.Size = new System.Drawing.Size(71, 17);
			this.rbtnRecurring.TabIndex = 0;
			this.rbtnRecurring.TabStop = true;
			this.rbtnRecurring.Text = "Recurring";
			this.rbtnRecurring.UseVisualStyleBackColor = true;
			this.rbtnRecurring.CheckedChanged += new System.EventHandler(this.ScheduleTypeChanged);
			// 
			// BackupPlanScheduleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "BackupPlanScheduleForm";
			this.Text = "Backup Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.flpSupportedAccounts.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Panel panelItemAmazonS3;
		protected System.Windows.Forms.FlowLayoutPanel flpSupportedAccounts;
		protected System.Windows.Forms.Panel panel1;
		protected Common.Controls.GroupableRadioButton rbtnManual;
		protected System.Windows.Forms.Panel panel2;
		protected Common.Controls.GroupableRadioButton rbtnSpecific;
		protected System.Windows.Forms.Panel panel3;
		protected Common.Controls.GroupableRadioButton rbtnRecurring;
		private System.Windows.Forms.DateTimePicker dtpSpecificDate;
		private System.Windows.Forms.DateTimePicker dtpSpecificTime;
		private Common.Controls.RadioButtonGroup radioButtonGroupSupportedScheduleTypes;
		private System.Windows.Forms.LinkLabel llblEditSchedule;
	}
}