namespace DemoTransferPerformance
{
	partial class UploadPerfTestControl
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
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(211, 74);
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
			this.label1.Location = new System.Drawing.Point(15, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(23, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "File";
			// 
			// btnOpenFileDialog
			// 
			this.btnOpenFileDialog.Location = new System.Drawing.Point(259, 3);
			this.btnOpenFileDialog.Name = "btnOpenFileDialog";
			this.btnOpenFileDialog.Size = new System.Drawing.Size(27, 23);
			this.btnOpenFileDialog.TabIndex = 10;
			this.btnOpenFileDialog.Text = "...";
			this.btnOpenFileDialog.UseVisualStyleBackColor = true;
			this.btnOpenFileDialog.Click += new System.EventHandler(this.btnOpenFileDialog_Click);
			// 
			// txtFilePath
			// 
			this.txtFilePath.Location = new System.Drawing.Point(44, 3);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.Size = new System.Drawing.Size(209, 20);
			this.txtFilePath.TabIndex = 9;
			// 
			// UploadPerfTestControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnOpenFileDialog);
			this.Controls.Add(this.txtFilePath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnStart);
			this.Name = "UploadPerfTestControl";
			this.Size = new System.Drawing.Size(290, 100);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOpenFileDialog;
		private System.Windows.Forms.TextBox txtFilePath;
	}
}
