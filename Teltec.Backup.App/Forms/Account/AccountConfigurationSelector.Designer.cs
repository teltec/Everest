namespace Teltec.Backup.App.Forms.Account
{
	partial class AccountConfigurationSelector
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
			this.btnConfirm = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cbCreateNew = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbExistingConfigurations = new System.Windows.Forms.ComboBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnConfirm
			// 
			this.btnConfirm.Location = new System.Drawing.Point(252, 135);
			this.btnConfirm.Name = "btnConfirm";
			this.btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.btnConfirm.TabIndex = 0;
			this.btnConfirm.Text = "Confirm";
			this.btnConfirm.UseVisualStyleBackColor = true;
			this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cbCreateNew);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.cbExistingConfigurations);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(339, 129);
			this.panel1.TabIndex = 1;
			// 
			// cbCreateNew
			// 
			this.cbCreateNew.AutoSize = true;
			this.cbCreateNew.Location = new System.Drawing.Point(12, 83);
			this.cbCreateNew.Name = "cbCreateNew";
			this.cbCreateNew.Size = new System.Drawing.Size(183, 17);
			this.cbCreateNew.TabIndex = 2;
			this.cbCreateNew.Text = "Create new backup configuration";
			this.cbCreateNew.UseVisualStyleBackColor = true;
			this.cbCreateNew.CheckedChanged += new System.EventHandler(this.cbCreateNew_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(304, 26);
			this.label1.TabIndex = 1;
			this.label1.Text = "A backup configuration already exists for the selected account.\r\nIf you want to c" +
    "ontinue using it, select it on the list below: ";
			// 
			// cbExistingConfigurations
			// 
			this.cbExistingConfigurations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbExistingConfigurations.Location = new System.Drawing.Point(12, 44);
			this.cbExistingConfigurations.Name = "cbExistingConfigurations";
			this.cbExistingConfigurations.Size = new System.Drawing.Size(315, 21);
			this.cbExistingConfigurations.TabIndex = 0;
			// 
			// AccountConfigurationSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(339, 170);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.btnConfirm);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AccountConfigurationSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Account > Select configuration";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnConfirm;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ComboBox cbExistingConfigurations;
		private System.Windows.Forms.CheckBox cbCreateNew;
		private System.Windows.Forms.Label label1;
	}
}