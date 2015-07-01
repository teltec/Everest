namespace Teltec.Backup.App.Forms.Sync
{
	partial class SyncProgressForm
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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lblLastSuccessfulRun = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.lblDuration = new System.Windows.Forms.Label();
			this.lblFilesSynced = new System.Windows.Forms.Label();
			this.lblLastRun = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblRemoteDirectory = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.panelActions = new System.Windows.Forms.Panel();
			this.llblRunNow = new System.Windows.Forms.LinkLabel();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panelActions.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.panelActions);
			this.groupBox1.Controls.Add(this.groupBox2);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lblLastSuccessfulRun);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.lblDuration);
			this.groupBox2.Controls.Add(this.lblFilesSynced);
			this.groupBox2.Controls.Add(this.lblLastRun);
			this.groupBox2.Controls.Add(this.lblStatus);
			this.groupBox2.Controls.Add(this.lblRemoteDirectory);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox2.Location = new System.Drawing.Point(5, 18);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(409, 134);
			this.groupBox2.TabIndex = 14;
			this.groupBox2.TabStop = false;
			// 
			// lblLastSuccessfulRun
			// 
			this.lblLastSuccessfulRun.AutoSize = true;
			this.lblLastSuccessfulRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLastSuccessfulRun.Location = new System.Drawing.Point(132, 70);
			this.lblLastSuccessfulRun.Margin = new System.Windows.Forms.Padding(3);
			this.lblLastSuccessfulRun.Name = "lblLastSuccessfulRun";
			this.lblLastSuccessfulRun.Size = new System.Drawing.Size(121, 13);
			this.lblLastSuccessfulRun.TabIndex = 25;
			this.lblLastSuccessfulRun.Text = "{{ LastSuccessfulRun }}";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(6, 70);
			this.label8.Margin = new System.Windows.Forms.Padding(3);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(117, 13);
			this.label8.TabIndex = 24;
			this.label8.Text = "Last successful run";
			// 
			// lblDuration
			// 
			this.lblDuration.AutoSize = true;
			this.lblDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDuration.Location = new System.Drawing.Point(132, 108);
			this.lblDuration.Margin = new System.Windows.Forms.Padding(3);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(69, 13);
			this.lblDuration.TabIndex = 23;
			this.lblDuration.Text = "{{ Duration }}";
			// 
			// lblFilesSynced
			// 
			this.lblFilesSynced.AutoSize = true;
			this.lblFilesSynced.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblFilesSynced.Location = new System.Drawing.Point(132, 89);
			this.lblFilesSynced.Margin = new System.Windows.Forms.Padding(3);
			this.lblFilesSynced.Name = "lblFilesSynced";
			this.lblFilesSynced.Size = new System.Drawing.Size(86, 13);
			this.lblFilesSynced.TabIndex = 22;
			this.lblFilesSynced.Text = "{{ FilesSynced }}";
			// 
			// lblLastRun
			// 
			this.lblLastRun.AutoSize = true;
			this.lblLastRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLastRun.Location = new System.Drawing.Point(132, 51);
			this.lblLastRun.Margin = new System.Windows.Forms.Padding(3);
			this.lblLastRun.Name = "lblLastRun";
			this.lblLastRun.Size = new System.Drawing.Size(69, 13);
			this.lblLastRun.TabIndex = 21;
			this.lblLastRun.Text = "{{ LastRun }}";
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblStatus.Location = new System.Drawing.Point(132, 32);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(3);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(59, 13);
			this.lblStatus.TabIndex = 20;
			this.lblStatus.Text = "{{ Status }}";
			// 
			// lblRemoteDirectory
			// 
			this.lblRemoteDirectory.AutoSize = true;
			this.lblRemoteDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblRemoteDirectory.Location = new System.Drawing.Point(132, 13);
			this.lblRemoteDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.lblRemoteDirectory.Name = "lblRemoteDirectory";
			this.lblRemoteDirectory.Size = new System.Drawing.Size(108, 13);
			this.lblRemoteDirectory.TabIndex = 18;
			this.lblRemoteDirectory.Text = "{{ RemoteDirectory }}";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(6, 108);
			this.label6.Margin = new System.Windows.Forms.Padding(3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(85, 13);
			this.label6.TabIndex = 17;
			this.label6.Text = "Sync duration";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(6, 89);
			this.label5.Margin = new System.Windows.Forms.Padding(3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Files synced";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(6, 51);
			this.label4.Margin = new System.Windows.Forms.Padding(3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(53, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "Last run";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(6, 32);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "Status";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(6, 13);
			this.label7.Margin = new System.Windows.Forms.Padding(3);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(103, 13);
			this.label7.TabIndex = 12;
			this.label7.Text = "Remote directory";
			// 
			// panelActions
			// 
			this.panelActions.Controls.Add(this.llblRunNow);
			this.panelActions.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelActions.Location = new System.Drawing.Point(5, 273);
			this.panelActions.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.panelActions.Name = "panelActions";
			this.panelActions.Size = new System.Drawing.Size(409, 28);
			this.panelActions.TabIndex = 26;
			// 
			// llblRunNow
			// 
			this.llblRunNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.llblRunNow.AutoSize = true;
			this.llblRunNow.Location = new System.Drawing.Point(353, 8);
			this.llblRunNow.Name = "llblRunNow";
			this.llblRunNow.Size = new System.Drawing.Size(50, 13);
			this.llblRunNow.TabIndex = 15;
			this.llblRunNow.TabStop = true;
			this.llblRunNow.Text = "Run now";
			this.llblRunNow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblRunNow_LinkClicked);
			// 
			// timer1
			// 
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// SyncProgressForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "SyncProgressForm";
			this.Text = "Synchronization";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panelActions.ResumeLayout(false);
			this.panelActions.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label lblLastSuccessfulRun;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label lblDuration;
		private System.Windows.Forms.Label lblFilesSynced;
		private System.Windows.Forms.Label lblLastRun;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblRemoteDirectory;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panelActions;
		private System.Windows.Forms.LinkLabel llblRunNow;
		private System.Windows.Forms.Timer timer1;
	}
}