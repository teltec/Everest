namespace DemoTransferS3
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
			this.tbAccessKey = new System.Windows.Forms.TextBox();
			this.tbSecretKey = new System.Windows.Forms.TextBox();
			this.tbBucketName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnBackup = new System.Windows.Forms.Button();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.transferListControl1 = new Teltec.Storage.Monitor.TransferListControl();
			this.btnRestore = new System.Windows.Forms.Button();
			this.nudParallelism = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.label5 = new System.Windows.Forms.Label();
			this.txtSourceDirectory = new Teltec.Common.Controls.TextBoxSelectFolderDialog();
			this.cbSimulateFailure = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.nudParallelism)).BeginInit();
			this.SuspendLayout();
			// 
			// tbAccessKey
			// 
			this.tbAccessKey.Location = new System.Drawing.Point(98, 12);
			this.tbAccessKey.Name = "tbAccessKey";
			this.tbAccessKey.Size = new System.Drawing.Size(205, 20);
			this.tbAccessKey.TabIndex = 0;
			// 
			// tbSecretKey
			// 
			this.tbSecretKey.Location = new System.Drawing.Point(98, 38);
			this.tbSecretKey.Name = "tbSecretKey";
			this.tbSecretKey.PasswordChar = '●';
			this.tbSecretKey.Size = new System.Drawing.Size(205, 20);
			this.tbSecretKey.TabIndex = 1;
			// 
			// tbBucketName
			// 
			this.tbBucketName.Location = new System.Drawing.Point(98, 64);
			this.tbBucketName.Name = "tbBucketName";
			this.tbBucketName.Size = new System.Drawing.Size(205, 20);
			this.tbBucketName.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Access key";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Secret key";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 67);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Bucket name";
			// 
			// btnBackup
			// 
			this.btnBackup.Location = new System.Drawing.Point(309, 12);
			this.btnBackup.Name = "btnBackup";
			this.btnBackup.Size = new System.Drawing.Size(75, 23);
			this.btnBackup.TabIndex = 6;
			this.btnBackup.Text = "Backup";
			this.btnBackup.UseVisualStyleBackColor = true;
			this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
			// 
			// listBox1
			// 
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(0, 174);
			this.listBox1.Name = "listBox1";
			this.listBox1.ScrollAlwaysVisible = true;
			this.listBox1.Size = new System.Drawing.Size(743, 173);
			this.listBox1.TabIndex = 7;
			// 
			// transferListControl1
			// 
			this.transferListControl1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.transferListControl1.CausesValidation = false;
			this.transferListControl1.Location = new System.Drawing.Point(390, 12);
			this.transferListControl1.Name = "transferListControl1";
			this.transferListControl1.Size = new System.Drawing.Size(341, 125);
			this.transferListControl1.TabIndex = 8;
			// 
			// btnRestore
			// 
			this.btnRestore.Location = new System.Drawing.Point(309, 41);
			this.btnRestore.Name = "btnRestore";
			this.btnRestore.Size = new System.Drawing.Size(75, 23);
			this.btnRestore.TabIndex = 9;
			this.btnRestore.Text = "Restore";
			this.btnRestore.UseVisualStyleBackColor = true;
			this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
			// 
			// nudParallelism
			// 
			this.nudParallelism.Location = new System.Drawing.Point(98, 117);
			this.nudParallelism.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.nudParallelism.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudParallelism.Name = "nudParallelism";
			this.nudParallelism.Size = new System.Drawing.Size(205, 20);
			this.nudParallelism.TabIndex = 10;
			this.nudParallelism.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 119);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Parallelism";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 93);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(84, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Source directory";
			// 
			// txtSourceDirectory
			// 
			this.txtSourceDirectory.Location = new System.Drawing.Point(98, 86);
			this.txtSourceDirectory.Name = "txtSourceDirectory";
			this.txtSourceDirectory.RootFolder = System.Environment.SpecialFolder.MyComputer;
			this.txtSourceDirectory.Size = new System.Drawing.Size(211, 30);
			this.txtSourceDirectory.TabIndex = 14;
			// 
			// cbSimulateFailure
			// 
			this.cbSimulateFailure.AutoSize = true;
			this.cbSimulateFailure.Location = new System.Drawing.Point(98, 143);
			this.cbSimulateFailure.Name = "cbSimulateFailure";
			this.cbSimulateFailure.Size = new System.Drawing.Size(228, 17);
			this.cbSimulateFailure.TabIndex = 15;
			this.cbSimulateFailure.Text = "Simulate failure (includes \"C:\\pagefile.sys\")";
			this.cbSimulateFailure.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AcceptButton = this.btnBackup;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(743, 347);
			this.Controls.Add(this.cbSimulateFailure);
			this.Controls.Add(this.txtSourceDirectory);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.nudParallelism);
			this.Controls.Add(this.btnRestore);
			this.Controls.Add(this.transferListControl1);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.btnBackup);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbBucketName);
			this.Controls.Add(this.tbSecretKey);
			this.Controls.Add(this.tbAccessKey);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Transfer to S3";
			((System.ComponentModel.ISupportInitialize)(this.nudParallelism)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tbAccessKey;
		private System.Windows.Forms.TextBox tbSecretKey;
		private System.Windows.Forms.TextBox tbBucketName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnBackup;
		private System.Windows.Forms.ListBox listBox1;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private Teltec.Storage.Monitor.TransferListControl transferListControl1;
		private System.Windows.Forms.Button btnRestore;
		private System.Windows.Forms.NumericUpDown nudParallelism;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Label label5;
		private Teltec.Common.Controls.TextBoxSelectFolderDialog txtSourceDirectory;
		private System.Windows.Forms.CheckBox cbSimulateFailure;
	}
}

