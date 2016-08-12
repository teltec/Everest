namespace DemoTransferPerformance
{
	partial class Form1
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
			this.uploadPerfTestControl1 = new DemoTransferPerformance.UploadPerfTestControl();
			this.SuspendLayout();
			// 
			// uploadPerfTestControl1
			// 
			this.uploadPerfTestControl1.AutoSize = true;
			this.uploadPerfTestControl1.Location = new System.Drawing.Point(12, 12);
			this.uploadPerfTestControl1.Name = "uploadPerfTestControl1";
			this.uploadPerfTestControl1.Size = new System.Drawing.Size(290, 135);
			this.uploadPerfTestControl1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(517, 261);
			this.Controls.Add(this.uploadPerfTestControl1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UploadPerfTestControl uploadPerfTestControl1;
	}
}

