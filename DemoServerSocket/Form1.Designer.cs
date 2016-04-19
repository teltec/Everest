namespace ServerSocketSim
{
	partial class Form1
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
			this.txtInput = new System.Windows.Forms.TextBox();
			this.lbxHistory = new System.Windows.Forms.ListBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// txtInput
			//
			this.txtInput.Location = new System.Drawing.Point(12, 220);
			this.txtInput.Name = "txtInput";
			this.txtInput.Size = new System.Drawing.Size(413, 20);
			this.txtInput.TabIndex = 5;
			this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyUp);
			//
			// lbxHistory
			//
			this.lbxHistory.FormattingEnabled = true;
			this.lbxHistory.Location = new System.Drawing.Point(12, 41);
			this.lbxHistory.Name = "lbxHistory";
			this.lbxHistory.Size = new System.Drawing.Size(413, 173);
			this.lbxHistory.TabIndex = 4;
			//
			// btnStart
			//
			this.btnStart.Location = new System.Drawing.Point(12, 12);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 3;
			this.btnStart.Text = "start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(437, 261);
			this.Controls.Add(this.txtInput);
			this.Controls.Add(this.lbxHistory);
			this.Controls.Add(this.btnStart);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtInput;
		private System.Windows.Forms.ListBox lbxHistory;
		private System.Windows.Forms.Button btnStart;
	}
}

