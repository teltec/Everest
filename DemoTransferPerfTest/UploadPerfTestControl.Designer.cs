namespace DemoTransferPerformance
{
	partial class UploadPerfTestControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnStart = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.btnOpenFileDialog = new System.Windows.Forms.Button();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtAccessKey = new System.Windows.Forms.TextBox();
			this.txtSecretKey = new System.Windows.Forms.TextBox();
			this.txtBucketName = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(211, 109);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDlg";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 84);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(23, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "File";
			// 
			// btnOpenFileDialog
			// 
			this.btnOpenFileDialog.Location = new System.Drawing.Point(259, 80);
			this.btnOpenFileDialog.Name = "btnOpenFileDialog";
			this.btnOpenFileDialog.Size = new System.Drawing.Size(27, 23);
			this.btnOpenFileDialog.TabIndex = 10;
			this.btnOpenFileDialog.Text = "...";
			this.btnOpenFileDialog.UseVisualStyleBackColor = true;
			this.btnOpenFileDialog.Click += new System.EventHandler(this.btnOpenFileDialog_Click);
			// 
			// txtFilePath
			// 
			this.txtFilePath.Location = new System.Drawing.Point(85, 81);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.Size = new System.Drawing.Size(168, 20);
			this.txtFilePath.TabIndex = 9;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 33);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 13);
			this.label3.TabIndex = 12;
			this.label3.Text = "Secret Key";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "Access Key";
			// 
			// txtAccessKey
			// 
			this.txtAccessKey.Location = new System.Drawing.Point(85, 9);
			this.txtAccessKey.Name = "txtAccessKey";
			this.txtAccessKey.Size = new System.Drawing.Size(168, 20);
			this.txtAccessKey.TabIndex = 13;
			// 
			// txtSecretKey
			// 
			this.txtSecretKey.Location = new System.Drawing.Point(85, 33);
			this.txtSecretKey.Name = "txtSecretKey";
			this.txtSecretKey.PasswordChar = '*';
			this.txtSecretKey.Size = new System.Drawing.Size(168, 20);
			this.txtSecretKey.TabIndex = 14;
			// 
			// txtBucketName
			// 
			this.txtBucketName.Location = new System.Drawing.Point(85, 57);
			this.txtBucketName.Name = "txtBucketName";
			this.txtBucketName.Size = new System.Drawing.Size(168, 20);
			this.txtBucketName.TabIndex = 16;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(15, 57);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(41, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "Bucket";
			// 
			// UploadPerfTestControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.txtBucketName);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtSecretKey);
			this.Controls.Add(this.txtAccessKey);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnOpenFileDialog);
			this.Controls.Add(this.txtFilePath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnStart);
			this.Name = "UploadPerfTestControl";
			this.Size = new System.Drawing.Size(290, 138);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOpenFileDialog;
		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtAccessKey;
		private System.Windows.Forms.TextBox txtSecretKey;
		private System.Windows.Forms.TextBox txtBucketName;
		private System.Windows.Forms.Label label4;
	}
}
