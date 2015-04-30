namespace Teltec.Backup.App.Forms.Schedule
{
	partial class ScheduleRecurringOptionsForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbFrequencyType = new System.Windows.Forms.ComboBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.dudUnit = new System.Windows.Forms.DomainUpDown();
			this.dtpTo = new System.Windows.Forms.DateTimePicker();
			this.label3 = new System.Windows.Forms.Label();
			this.dtpFrom = new System.Windows.Forms.DateTimePicker();
			this.label1 = new System.Windows.Forms.Label();
			this.nudInterval = new System.Windows.Forms.NumericUpDown();
			this.rbtnOccursEvery = new System.Windows.Forms.RadioButton();
			this.dtpOccursAt = new System.Windows.Forms.DateTimePicker();
			this.rbtnOccursAt = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnConfirm = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudInterval)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.AutoSize = true;
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.cbFrequencyType);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(501, 111);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Frequency";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(19, 21);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(31, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Type";
			// 
			// cbFrequencyType
			// 
			this.cbFrequencyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbFrequencyType.Items.AddRange(new object[] {
            "Daily",
            "Weekly",
            "Monthly",
            "Day of Month"});
			this.cbFrequencyType.Location = new System.Drawing.Point(103, 18);
			this.cbFrequencyType.Name = "cbFrequencyType";
			this.cbFrequencyType.Size = new System.Drawing.Size(121, 21);
			this.cbFrequencyType.TabIndex = 5;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.AutoSize = true;
			this.groupBox2.Controls.Add(this.dudUnit);
			this.groupBox2.Controls.Add(this.dtpTo);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.dtpFrom);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.nudInterval);
			this.groupBox2.Controls.Add(this.rbtnOccursEvery);
			this.groupBox2.Controls.Add(this.dtpOccursAt);
			this.groupBox2.Controls.Add(this.rbtnOccursAt);
			this.groupBox2.Location = new System.Drawing.Point(12, 129);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(501, 110);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Daily frequency";
			// 
			// dudUnit
			// 
			this.dudUnit.Items.Add("minute(s)");
			this.dudUnit.Items.Add("hour(s)");
			this.dudUnit.Location = new System.Drawing.Point(230, 45);
			this.dudUnit.Name = "dudUnit";
			this.dudUnit.Size = new System.Drawing.Size(75, 20);
			this.dudUnit.TabIndex = 16;
			// 
			// dtpTo
			// 
			this.dtpTo.CustomFormat = "HH:mm";
			this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpTo.Location = new System.Drawing.Point(386, 71);
			this.dtpTo.Name = "dtpTo";
			this.dtpTo.ShowUpDown = true;
			this.dtpTo.Size = new System.Drawing.Size(75, 20);
			this.dtpTo.TabIndex = 15;
			this.dtpTo.Value = new System.DateTime(2015, 4, 30, 23, 59, 0, 0);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(340, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(20, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "To";
			// 
			// dtpFrom
			// 
			this.dtpFrom.CustomFormat = "HH:mm";
			this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpFrom.Location = new System.Drawing.Point(386, 45);
			this.dtpFrom.Name = "dtpFrom";
			this.dtpFrom.ShowUpDown = true;
			this.dtpFrom.Size = new System.Drawing.Size(75, 20);
			this.dtpFrom.TabIndex = 13;
			this.dtpFrom.Value = new System.DateTime(2015, 4, 30, 0, 0, 0, 0);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(340, 49);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(30, 13);
			this.label1.TabIndex = 12;
			this.label1.Text = "From";
			// 
			// nudInterval
			// 
			this.nudInterval.Location = new System.Drawing.Point(149, 45);
			this.nudInterval.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
			this.nudInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudInterval.Name = "nudInterval";
			this.nudInterval.Size = new System.Drawing.Size(75, 20);
			this.nudInterval.TabIndex = 10;
			this.nudInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// rbtnOccursEvery
			// 
			this.rbtnOccursEvery.AutoSize = true;
			this.rbtnOccursEvery.Location = new System.Drawing.Point(22, 45);
			this.rbtnOccursEvery.Name = "rbtnOccursEvery";
			this.rbtnOccursEvery.Size = new System.Drawing.Size(88, 17);
			this.rbtnOccursEvery.TabIndex = 9;
			this.rbtnOccursEvery.Text = "Occurs every";
			this.rbtnOccursEvery.UseVisualStyleBackColor = true;
			// 
			// dtpOccursAt
			// 
			this.dtpOccursAt.CustomFormat = "HH:mm";
			this.dtpOccursAt.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpOccursAt.Location = new System.Drawing.Point(149, 19);
			this.dtpOccursAt.Name = "dtpOccursAt";
			this.dtpOccursAt.ShowUpDown = true;
			this.dtpOccursAt.Size = new System.Drawing.Size(75, 20);
			this.dtpOccursAt.TabIndex = 8;
			this.dtpOccursAt.Value = new System.DateTime(2015, 4, 30, 0, 0, 0, 0);
			// 
			// rbtnOccursAt
			// 
			this.rbtnOccursAt.AutoSize = true;
			this.rbtnOccursAt.Checked = true;
			this.rbtnOccursAt.Location = new System.Drawing.Point(22, 22);
			this.rbtnOccursAt.Name = "rbtnOccursAt";
			this.rbtnOccursAt.Size = new System.Drawing.Size(71, 17);
			this.rbtnOccursAt.TabIndex = 7;
			this.rbtnOccursAt.TabStop = true;
			this.rbtnOccursAt.Text = "Occurs at";
			this.rbtnOccursAt.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.Controls.Add(this.btnConfirm);
			this.panel1.Controls.Add(this.btnCancel);
			this.panel1.Location = new System.Drawing.Point(12, 245);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(503, 29);
			this.panel1.TabIndex = 7;
			// 
			// btnConfirm
			// 
			this.btnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnConfirm.Location = new System.Drawing.Point(344, 3);
			this.btnConfirm.Name = "btnConfirm";
			this.btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.btnConfirm.TabIndex = 2;
			this.btnConfirm.Text = "OK";
			this.btnConfirm.UseVisualStyleBackColor = true;
			this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(425, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// ScheduleRecurringOptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 282);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScheduleRecurringOptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Schedule Recurring Options";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudInterval)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbFrequencyType;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.DateTimePicker dtpTo;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker dtpFrom;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown nudInterval;
		private System.Windows.Forms.RadioButton rbtnOccursEvery;
		private System.Windows.Forms.DateTimePicker dtpOccursAt;
		private System.Windows.Forms.RadioButton rbtnOccursAt;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnConfirm;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.DomainUpDown dudUnit;

	}
}