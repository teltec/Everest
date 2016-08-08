using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.Notification
{
	partial class NotificationOptionsForm<T> where T : Models.SchedulablePlan<T>, new()
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
			this.cbNotificationEnabled = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.txtEmailSubject = new System.Windows.Forms.TextBox();
			this.txtFullName = new System.Windows.Forms.TextBox();
			this.txtEmailAddress = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.rbtnWhenItFails = new Teltec.Common.Controls.GroupableRadioButton();
			this.rbtnAlways = new Teltec.Common.Controls.GroupableRadioButton();
			this.radioButtonGroup1 = new Teltec.Common.Controls.RadioButtonGroup(this.components);
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.Size = new System.Drawing.Size(97, 13);
			this.label1.Text = "Notification options";
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this.panel1);
			//
			// cbNotificationEnabled
			//
			this.cbNotificationEnabled.AutoSize = true;
			this.cbNotificationEnabled.Location = new System.Drawing.Point(8, 8);
			this.cbNotificationEnabled.Name = "cbNotificationEnabled";
			this.cbNotificationEnabled.Size = new System.Drawing.Size(268, 17);
			this.cbNotificationEnabled.TabIndex = 5;
			this.cbNotificationEnabled.Text = "Receive notifications when {{operation}} completes";
			this.cbNotificationEnabled.UseVisualStyleBackColor = true;
			//
			// panel1
			//
			this.panel1.Controls.Add(this.txtEmailSubject);
			this.panel1.Controls.Add(this.txtFullName);
			this.panel1.Controls.Add(this.txtEmailAddress);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.rbtnWhenItFails);
			this.panel1.Controls.Add(this.rbtnAlways);
			this.panel1.Controls.Add(this.cbNotificationEnabled);
			this.panel1.Location = new System.Drawing.Point(5, 20);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 240);
			this.panel1.TabIndex = 2;
			//
			// txtEmailSubject
			//
			this.txtEmailSubject.Location = new System.Drawing.Point(105, 137);
			this.txtEmailSubject.Name = "txtEmailSubject";
			this.txtEmailSubject.Size = new System.Drawing.Size(296, 20);
			this.txtEmailSubject.TabIndex = 14;
			//
			// txtFullName
			//
			this.txtFullName.Location = new System.Drawing.Point(105, 111);
			this.txtFullName.Name = "txtFullName";
			this.txtFullName.Size = new System.Drawing.Size(296, 20);
			this.txtFullName.TabIndex = 13;
			//
			// txtEmailAddress
			//
			this.txtEmailAddress.Location = new System.Drawing.Point(105, 85);
			this.txtEmailAddress.Name = "txtEmailAddress";
			this.txtEmailAddress.Size = new System.Drawing.Size(296, 20);
			this.txtEmailAddress.TabIndex = 12;
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(24, 140);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(69, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Email subject";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(24, 114);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Full name";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(24, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Email";
			//
			// rbtnWhenItFails
			//
			this.rbtnWhenItFails.AutoSize = true;
			this.rbtnWhenItFails.Checked = true;
			this.rbtnWhenItFails.Location = new System.Drawing.Point(27, 31);
			this.rbtnWhenItFails.Name = "rbtnWhenItFails";
			this.rbtnWhenItFails.RadioGroup = null;
			this.rbtnWhenItFails.Size = new System.Drawing.Size(83, 17);
			this.rbtnWhenItFails.TabIndex = 8;
			this.rbtnWhenItFails.TabStop = true;
			this.rbtnWhenItFails.Text = "When it fails";
			this.rbtnWhenItFails.UseVisualStyleBackColor = true;
			this.rbtnWhenItFails.CheckedChanged += new System.EventHandler(this.WhenToNotifyChanged);
			//
			// rbtnAlways
			//
			this.rbtnAlways.AutoSize = true;
			this.rbtnAlways.Location = new System.Drawing.Point(27, 55);
			this.rbtnAlways.Name = "rbtnAlways";
			this.rbtnAlways.RadioGroup = null;
			this.rbtnAlways.Size = new System.Drawing.Size(58, 17);
			this.rbtnAlways.TabIndex = 7;
			this.rbtnAlways.Text = "Always";
			this.rbtnAlways.UseVisualStyleBackColor = true;
			this.rbtnAlways.CheckedChanged += new System.EventHandler(this.WhenToNotifyChanged);
			//
			// NotificationOptionsForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "NotificationOptionsForm";
			this.Text = "Backup Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox cbNotificationEnabled;
		private Common.Controls.RadioButtonGroup radioButtonGroup1;
		private Common.Controls.GroupableRadioButton rbtnAlways;
		private Common.Controls.GroupableRadioButton rbtnWhenItFails;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtEmailSubject;
		private System.Windows.Forms.TextBox txtFullName;
		private System.Windows.Forms.TextBox txtEmailAddress;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;

	}
}
