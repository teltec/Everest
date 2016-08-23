namespace Teltec.Everest.App.Forms.Settings
{
	partial class SettingsForm
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
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.nudUploadChunkSize = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.nudMaxThreads = new System.Windows.Forms.NumericUpDown();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudUploadChunkSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxThreads)).BeginInit();
			this.SuspendLayout();
			// 
			// btnApply
			// 
			this.btnApply.Location = new System.Drawing.Point(332, 226);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(75, 23);
			this.btnApply.TabIndex = 0;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(251, 226);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.nudUploadChunkSize);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.nudMaxThreads);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(395, 208);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(240, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(25, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "MiB";
			// 
			// nudUploadChunkSize
			// 
			this.nudUploadChunkSize.Location = new System.Drawing.Point(114, 49);
			this.nudUploadChunkSize.Maximum = new decimal(new int[] {
            5120,
            0,
            0,
            0});
			this.nudUploadChunkSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudUploadChunkSize.Name = "nudUploadChunkSize";
			this.nudUploadChunkSize.Size = new System.Drawing.Size(120, 20);
			this.nudUploadChunkSize.TabIndex = 3;
			this.nudUploadChunkSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 51);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Upload chunk size";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Maximum threads";
			// 
			// nudMaxThreads
			// 
			this.nudMaxThreads.Location = new System.Drawing.Point(114, 14);
			this.nudMaxThreads.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.nudMaxThreads.Name = "nudMaxThreads";
			this.nudMaxThreads.Size = new System.Drawing.Size(120, 20);
			this.nudMaxThreads.TabIndex = 0;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnApply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(419, 261);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnApply);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Settings";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudUploadChunkSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxThreads)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown nudMaxThreads;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown nudUploadChunkSize;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}
