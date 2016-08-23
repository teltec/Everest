namespace Teltec.Everest.App.Forms.S3
{
    partial class AmazonS3AccountForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.cbBucketName = new System.Windows.Forms.ComboBox();
			this.tbDisplayName = new System.Windows.Forms.TextBox();
			this.tbAccessKey = new System.Windows.Forms.TextBox();
			this.tbSecretKey = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.amazonS3AccountBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.amazonS3AccountBindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 159F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.cbBucketName, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.tbDisplayName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.tbAccessKey, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.tbSecretKey, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 6);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(395, 261);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(13, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(153, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "Display name";
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
			this.label2.Text = "Access key";
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
			this.label3.Text = "Secret key";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(13, 130);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(153, 30);
			this.label4.TabIndex = 3;
			this.label4.Text = "Bucket name";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// cbBucketName
			//
			this.cbBucketName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbBucketName.FormattingEnabled = true;
			this.cbBucketName.Items.AddRange(new object[] {
            "<Create new bucket>"});
			this.cbBucketName.Location = new System.Drawing.Point(172, 133);
			this.cbBucketName.Name = "cbBucketName";
			this.cbBucketName.Size = new System.Drawing.Size(210, 21);
			this.cbBucketName.TabIndex = 7;
			this.cbBucketName.DropDown += new System.EventHandler(this.cbBucketName_DropDown);
			this.cbBucketName.SelectionChangeCommitted += new System.EventHandler(this.cbBucketName_SelectionChangeCommitted);
			//
			// tbDisplayName
			//
			this.tbDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbDisplayName.Location = new System.Drawing.Point(172, 43);
			this.tbDisplayName.Name = "tbDisplayName";
			this.tbDisplayName.Size = new System.Drawing.Size(210, 20);
			this.tbDisplayName.TabIndex = 4;
			//
			// tbAccessKey
			//
			this.tbAccessKey.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbAccessKey.Location = new System.Drawing.Point(172, 73);
			this.tbAccessKey.Name = "tbAccessKey";
			this.tbAccessKey.Size = new System.Drawing.Size(210, 20);
			this.tbAccessKey.TabIndex = 5;
			//
			// tbSecretKey
			//
			this.tbSecretKey.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbSecretKey.Location = new System.Drawing.Point(172, 103);
			this.tbSecretKey.Name = "tbSecretKey";
			this.tbSecretKey.PasswordChar = '*';
			this.tbSecretKey.Size = new System.Drawing.Size(210, 20);
			this.tbSecretKey.TabIndex = 6;
			this.tbSecretKey.UseSystemPasswordChar = true;
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
			// amazonS3AccountBindingSource
			//
			this.amazonS3AccountBindingSource.DataSource = typeof(Teltec.Everest.Data.Models.AmazonS3Account);
			//
			// AmazonS3AccountForm
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
			this.Name = "AmazonS3AccountForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Amazon S3 Account";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.amazonS3AccountBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbBucketName;
        private System.Windows.Forms.TextBox tbDisplayName;
        private System.Windows.Forms.TextBox tbAccessKey;
        private System.Windows.Forms.TextBox tbSecretKey;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.BindingSource amazonS3AccountBindingSource;
    }
}
