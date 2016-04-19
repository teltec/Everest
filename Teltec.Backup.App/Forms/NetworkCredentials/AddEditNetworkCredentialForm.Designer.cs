namespace Teltec.Backup.App.Forms.NetworkCredentials
{
	partial class AddEditNetworkCredentialForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbMountPoint = new Teltec.Backup.App.Controls.DriveItemsComboBox();
			this.drivesBindingSource1 = new Teltec.Backup.App.Controls.DriveItemsBindingSource(this.components);
			this.btnOpenNetworkPathDialog = new System.Windows.Forms.Button();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.txtLogin = new System.Windows.Forms.TextBox();
			this.txtNetworkPath = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.drivesBindingSource1)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbMountPoint);
			this.groupBox1.Controls.Add(this.btnOpenNetworkPathDialog);
			this.groupBox1.Controls.Add(this.txtPassword);
			this.groupBox1.Controls.Add(this.txtLogin);
			this.groupBox1.Controls.Add(this.txtNetworkPath);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(395, 128);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			// 
			// cbMountPoint
			// 
			this.cbMountPoint.DataSource = this.drivesBindingSource1;
			this.cbMountPoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbMountPoint.FormattingEnabled = true;
			this.cbMountPoint.Location = new System.Drawing.Point(102, 43);
			this.cbMountPoint.Name = "cbMountPoint";
			this.cbMountPoint.Size = new System.Drawing.Size(211, 21);
			this.cbMountPoint.TabIndex = 9;
			this.cbMountPoint.SelectionChangeCommitted += new System.EventHandler(this.cbMountPoint_SelectionChangeCommitted);
			// 
			// drivesBindingSource1
			// 
			this.drivesBindingSource1.HideCDRomDrives = true;
			this.drivesBindingSource1.HideFixedDrives = true;
			this.drivesBindingSource1.HideNetworkDrives = false;
			this.drivesBindingSource1.HideRemovableDrives = true;
			// 
			// btnOpenNetworkPathDialog
			// 
			this.btnOpenNetworkPathDialog.Location = new System.Drawing.Point(287, 18);
			this.btnOpenNetworkPathDialog.Name = "btnOpenNetworkPathDialog";
			this.btnOpenNetworkPathDialog.Size = new System.Drawing.Size(27, 23);
			this.btnOpenNetworkPathDialog.TabIndex = 8;
			this.btnOpenNetworkPathDialog.Text = "...";
			this.btnOpenNetworkPathDialog.UseVisualStyleBackColor = true;
			this.btnOpenNetworkPathDialog.Click += new System.EventHandler(this.btnOpenNetworkPathDialog_Click);
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(102, 96);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(211, 20);
			this.txtPassword.TabIndex = 7;
			// 
			// txtLogin
			// 
			this.txtLogin.Location = new System.Drawing.Point(102, 70);
			this.txtLogin.Name = "txtLogin";
			this.txtLogin.Size = new System.Drawing.Size(211, 20);
			this.txtLogin.TabIndex = 6;
			// 
			// txtNetworkPath
			// 
			this.txtNetworkPath.Location = new System.Drawing.Point(102, 18);
			this.txtNetworkPath.Name = "txtNetworkPath";
			this.txtNetworkPath.Size = new System.Drawing.Size(179, 20);
			this.txtNetworkPath.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(15, 99);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(53, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Password";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 73);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(33, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Login";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 47);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Mount point";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Network path";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(251, 146);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(332, 146);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// AddEditNetworkCredentialForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(419, 181);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddEditNetworkCredentialForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add/Edit Network Credential";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.drivesBindingSource1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOpenNetworkPathDialog;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.TextBox txtLogin;
		private System.Windows.Forms.TextBox txtNetworkPath;
		private System.Windows.Forms.Label label4;
		private Controls.DriveItemsComboBox cbMountPoint;
		private Controls.DriveItemsBindingSource drivesBindingSource1;
	}
}
