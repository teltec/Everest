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
			this.panelItemAmazonS3 = new System.Windows.Forms.Panel();
			this.rbtnManual = new Teltec.Common.Controls.GroupableRadioButton();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panelItemAmazonS3.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(147, 13);
			this.label1.Text = "Define the scheduling options";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.panelItemAmazonS3);
			// 
			// panelItemAmazonS3
			// 
			this.panelItemAmazonS3.Controls.Add(this.rbtnManual);
			this.panelItemAmazonS3.Location = new System.Drawing.Point(5, 18);
			this.panelItemAmazonS3.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemAmazonS3.Name = "panelItemAmazonS3";
			this.panelItemAmazonS3.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemAmazonS3.Size = new System.Drawing.Size(409, 34);
			this.panelItemAmazonS3.TabIndex = 1;
			// 
			// rbtnManual
			// 
			this.rbtnManual.AutoSize = true;
			this.rbtnManual.Checked = true;
			this.rbtnManual.Location = new System.Drawing.Point(8, 8);
			this.rbtnManual.Name = "rbtnManual";
			this.rbtnManual.RadioGroup = null;
			this.rbtnManual.Size = new System.Drawing.Size(89, 17);
			this.rbtnManual.TabIndex = 0;
			this.rbtnManual.TabStop = true;
			this.rbtnManual.Text = "Run manually";
			this.rbtnManual.UseVisualStyleBackColor = true;
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
			this.panelItemAmazonS3.ResumeLayout(false);
			this.panelItemAmazonS3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Panel panelItemAmazonS3;
		protected Teltec.Common.Controls.GroupableRadioButton rbtnManual;
	}
}