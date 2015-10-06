namespace ClientSocketSim
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
			this.btnConnect = new System.Windows.Forms.Button();
			this.lbxHistory = new System.Windows.Forms.ListBox();
			this.txtInput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(12, 12);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(75, 23);
			this.btnConnect.TabIndex = 0;
			this.btnConnect.Text = "connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// lbxHistory
			// 
			this.lbxHistory.FormattingEnabled = true;
			this.lbxHistory.Location = new System.Drawing.Point(12, 41);
			this.lbxHistory.Name = "lbxHistory";
			this.lbxHistory.Size = new System.Drawing.Size(413, 173);
			this.lbxHistory.TabIndex = 1;
			// 
			// txtInput
			// 
			this.txtInput.Location = new System.Drawing.Point(12, 220);
			this.txtInput.Name = "txtInput";
			this.txtInput.Size = new System.Drawing.Size(413, 20);
			this.txtInput.TabIndex = 2;
			this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyUp);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(437, 261);
			this.Controls.Add(this.txtInput);
			this.Controls.Add(this.lbxHistory);
			this.Controls.Add(this.btnConnect);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.ListBox lbxHistory;
		private System.Windows.Forms.TextBox txtInput;
	}
}

