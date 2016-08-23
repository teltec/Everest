namespace Teltec.Everest.App.Forms.S3
{
	partial class AmazonS3CreateBucketForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.cbStorageClass = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.cbBucketLocation = new System.Windows.Forms.ComboBox();
			this.tbBucketName = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 159F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.cbStorageClass, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.cbBucketLocation, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.tbBucketName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 5);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(395, 261);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// cbStorageClass
			// 
			this.cbStorageClass.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbStorageClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbStorageClass.FormattingEnabled = true;
			this.cbStorageClass.Location = new System.Drawing.Point(172, 103);
			this.cbStorageClass.Name = "cbStorageClass";
			this.cbStorageClass.Size = new System.Drawing.Size(210, 21);
			this.cbStorageClass.TabIndex = 11;
			this.cbStorageClass.DropDown += new System.EventHandler(this.cbStorageClass_DropDown);
			this.cbStorageClass.SelectionChangeCommitted += new System.EventHandler(this.cbStorageClass_SelectionChangeCommitted);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(13, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(153, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "Bucket name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(13, 70);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(153, 30);
			this.label2.TabIndex = 1;
			this.label2.Text = "Bucket location";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(13, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(153, 30);
			this.label3.TabIndex = 2;
			this.label3.Text = "Amazon Storage Class";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbBucketLocation
			// 
			this.cbBucketLocation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbBucketLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbBucketLocation.FormattingEnabled = true;
			this.cbBucketLocation.Location = new System.Drawing.Point(172, 73);
			this.cbBucketLocation.Name = "cbBucketLocation";
			this.cbBucketLocation.Size = new System.Drawing.Size(210, 21);
			this.cbBucketLocation.TabIndex = 7;
			this.cbBucketLocation.DropDown += new System.EventHandler(this.cbBucketLocation_DropDown);
			this.cbBucketLocation.SelectionChangeCommitted += new System.EventHandler(this.cbBucketLocation_SelectionChangeCommitted);
			// 
			// tbBucketName
			// 
			this.tbBucketName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbBucketName.Location = new System.Drawing.Point(172, 43);
			this.tbBucketName.Name = "tbBucketName";
			this.tbBucketName.Size = new System.Drawing.Size(210, 20);
			this.tbBucketName.TabIndex = 4;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.Controls.Add(this.btnSave);
			this.panel1.Controls.Add(this.btnCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(172, 224);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(210, 24);
			this.panel1.TabIndex = 10;
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(44, 1);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 8;
			this.btnSave.Text = "OK";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(125, 1);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// AmazonS3CreateBucketForm
			// 
			this.AcceptButton = this.btnSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(395, 261);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AmazonS3CreateBucketForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Amazon S3 Create Bucket";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbBucketLocation;
		private System.Windows.Forms.TextBox tbBucketName;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbStorageClass;
	}
}
