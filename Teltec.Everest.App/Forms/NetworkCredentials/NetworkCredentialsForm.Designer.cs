namespace Teltec.Everest.App.Forms.NetworkCredentials
{
	partial class NetworkCredentialsForm
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
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnAdd = new System.Windows.Forms.Button();
			this.dgvCredentials = new System.Windows.Forms.DataGridView();
			this.colAccount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colMountPoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DataGridDataSource = new System.Windows.Forms.BindingSource(this.components);
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvCredentials)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DataGridDataSource)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnRemove);
			this.groupBox1.Controls.Add(this.btnEdit);
			this.groupBox1.Controls.Add(this.btnAdd);
			this.groupBox1.Controls.Add(this.dgvCredentials);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(395, 208);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			// 
			// btnRemove
			// 
			this.btnRemove.Location = new System.Drawing.Point(314, 70);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(75, 23);
			this.btnRemove.TabIndex = 3;
			this.btnRemove.Text = "Remove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.Location = new System.Drawing.Point(314, 41);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(75, 23);
			this.btnEdit.TabIndex = 2;
			this.btnEdit.Text = "Edit";
			this.btnEdit.UseVisualStyleBackColor = true;
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// btnAdd
			// 
			this.btnAdd.Location = new System.Drawing.Point(314, 12);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(75, 23);
			this.btnAdd.TabIndex = 1;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// dgvCredentials
			// 
			this.dgvCredentials.AllowUserToAddRows = false;
			this.dgvCredentials.AllowUserToDeleteRows = false;
			this.dgvCredentials.AllowUserToResizeRows = false;
			this.dgvCredentials.AutoGenerateColumns = false;
			this.dgvCredentials.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvCredentials.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAccount,
            this.colMountPoint,
            this.colPath});
			this.dgvCredentials.DataSource = this.DataGridDataSource;
			this.dgvCredentials.Location = new System.Drawing.Point(6, 12);
			this.dgvCredentials.MultiSelect = false;
			this.dgvCredentials.Name = "dgvCredentials";
			this.dgvCredentials.ReadOnly = true;
			this.dgvCredentials.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvCredentials.ShowCellToolTips = false;
			this.dgvCredentials.Size = new System.Drawing.Size(302, 186);
			this.dgvCredentials.TabIndex = 0;
			this.dgvCredentials.VirtualMode = true;
			this.dgvCredentials.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgvCredentials_MouseDoubleClick);
			// 
			// colAccount
			// 
			this.colAccount.DataPropertyName = "Login";
			this.colAccount.Frozen = true;
			this.colAccount.HeaderText = "Account";
			this.colAccount.Name = "colAccount";
			this.colAccount.ReadOnly = true;
			// 
			// colMountPoint
			// 
			this.colMountPoint.DataPropertyName = "MountPoint";
			this.colMountPoint.Frozen = true;
			this.colMountPoint.HeaderText = "Mount point";
			this.colMountPoint.Name = "colMountPoint";
			this.colMountPoint.ReadOnly = true;
			// 
			// colPath
			// 
			this.colPath.DataPropertyName = "Path";
			this.colPath.Frozen = true;
			this.colPath.HeaderText = "Path";
			this.colPath.Name = "colPath";
			this.colPath.ReadOnly = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(251, 226);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(332, 226);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// NetworkCredentialsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(419, 261);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NetworkCredentialsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Network Credentials";
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvCredentials)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DataGridDataSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.DataGridView dgvCredentials;
		private System.Windows.Forms.BindingSource DataGridDataSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn colAccount;
		private System.Windows.Forms.DataGridViewTextBoxColumn colMountPoint;
		private System.Windows.Forms.DataGridViewTextBoxColumn colPath;
	}
}
