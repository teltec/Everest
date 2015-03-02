namespace Teltec.Backup.App.Forms.BackupPlan
{
	partial class BackupPlanViewControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.llblEditPlan = new System.Windows.Forms.LinkLabel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblDuration = new System.Windows.Forms.Label();
			this.lblFilesUploaded = new System.Windows.Forms.Label();
			this.lblLastRun = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblSchedule = new System.Windows.Forms.Label();
			this.lblSources = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.llblDeletePlan = new System.Windows.Forms.LinkLabel();
			this.llblRunNow = new System.Windows.Forms.LinkLabel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.panelTitle = new System.Windows.Forms.Panel();
			this.panelActions = new System.Windows.Forms.Panel();
			this.panelContents = new System.Windows.Forms.Panel();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.groupBox1.SuspendLayout();
			this.panelTitle.SuspendLayout();
			this.panelActions.SuspendLayout();
			this.panelContents.SuspendLayout();
			this.SuspendLayout();
			// 
			// llblEditPlan
			// 
			this.llblEditPlan.AutoSize = true;
			this.llblEditPlan.Location = new System.Drawing.Point(8, 8);
			this.llblEditPlan.Name = "llblEditPlan";
			this.llblEditPlan.Size = new System.Drawing.Size(48, 13);
			this.llblEditPlan.TabIndex = 12;
			this.llblEditPlan.TabStop = true;
			this.llblEditPlan.Text = "Edit plan";
			this.llblEditPlan.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblEditPlan_LinkClicked);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lblDuration);
			this.groupBox1.Controls.Add(this.lblFilesUploaded);
			this.groupBox1.Controls.Add(this.lblLastRun);
			this.groupBox1.Controls.Add(this.lblStatus);
			this.groupBox1.Controls.Add(this.lblSchedule);
			this.groupBox1.Controls.Add(this.lblSources);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(398, 128);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
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
			// lblFilesUploaded
			// 
			this.lblFilesUploaded.AutoSize = true;
			this.lblFilesUploaded.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblFilesUploaded.Location = new System.Drawing.Point(132, 89);
			this.lblFilesUploaded.Margin = new System.Windows.Forms.Padding(3);
			this.lblFilesUploaded.Name = "lblFilesUploaded";
			this.lblFilesUploaded.Size = new System.Drawing.Size(96, 13);
			this.lblFilesUploaded.TabIndex = 22;
			this.lblFilesUploaded.Text = "{{ FilesUploaded }}";
			// 
			// lblLastRun
			// 
			this.lblLastRun.AutoSize = true;
			this.lblLastRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLastRun.Location = new System.Drawing.Point(132, 70);
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
			this.lblStatus.Location = new System.Drawing.Point(132, 51);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(3);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(59, 13);
			this.lblStatus.TabIndex = 20;
			this.lblStatus.Text = "{{ Status }}";
			// 
			// lblSchedule
			// 
			this.lblSchedule.AutoSize = true;
			this.lblSchedule.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSchedule.Location = new System.Drawing.Point(132, 32);
			this.lblSchedule.Margin = new System.Windows.Forms.Padding(3);
			this.lblSchedule.Name = "lblSchedule";
			this.lblSchedule.Size = new System.Drawing.Size(74, 13);
			this.lblSchedule.TabIndex = 19;
			this.lblSchedule.Text = "{{ Schedule }}";
			// 
			// lblSources
			// 
			this.lblSources.AutoSize = true;
			this.lblSources.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSources.Location = new System.Drawing.Point(132, 13);
			this.lblSources.Margin = new System.Windows.Forms.Padding(3);
			this.lblSources.Name = "lblSources";
			this.lblSources.Size = new System.Drawing.Size(68, 13);
			this.lblSources.TabIndex = 18;
			this.lblSources.Text = "{{ Sources }}";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(6, 108);
			this.label6.Margin = new System.Windows.Forms.Padding(3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(100, 13);
			this.label6.TabIndex = 17;
			this.label6.Text = "Backup duration";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(6, 89);
			this.label5.Margin = new System.Windows.Forms.Padding(3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(89, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Files uploaded";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(6, 70);
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
			this.label3.Location = new System.Drawing.Point(6, 51);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "Status";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(6, 32);
			this.label2.Margin = new System.Windows.Forms.Padding(3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 13;
			this.label2.Text = "Schedule";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(6, 13);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 13);
			this.label1.TabIndex = 12;
			this.label1.Text = "Backup";
			// 
			// llblDeletePlan
			// 
			this.llblDeletePlan.AutoSize = true;
			this.llblDeletePlan.Location = new System.Drawing.Point(62, 8);
			this.llblDeletePlan.Name = "llblDeletePlan";
			this.llblDeletePlan.Size = new System.Drawing.Size(61, 13);
			this.llblDeletePlan.TabIndex = 14;
			this.llblDeletePlan.TabStop = true;
			this.llblDeletePlan.Text = "Delete plan";
			this.llblDeletePlan.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblDeletePlan_LinkClicked);
			// 
			// llblRunNow
			// 
			this.llblRunNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.llblRunNow.AutoSize = true;
			this.llblRunNow.Location = new System.Drawing.Point(342, 8);
			this.llblRunNow.Name = "llblRunNow";
			this.llblRunNow.Size = new System.Drawing.Size(50, 13);
			this.llblRunNow.TabIndex = 15;
			this.llblRunNow.TabStop = true;
			this.llblRunNow.Text = "Run now";
			this.llblRunNow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblRunNow_LinkClicked);
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(11, 8);
			this.lblTitle.Margin = new System.Windows.Forms.Padding(3);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(70, 13);
			this.lblTitle.TabIndex = 24;
			this.lblTitle.Text = "{{ TITLE }}";
			this.lblTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelTitle_MouseClick);
			// 
			// panelTitle
			// 
			this.panelTitle.BackColor = System.Drawing.Color.LightGray;
			this.panelTitle.Controls.Add(this.lblTitle);
			this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTitle.Location = new System.Drawing.Point(8, 8);
			this.panelTitle.Name = "panelTitle";
			this.panelTitle.Size = new System.Drawing.Size(398, 28);
			this.panelTitle.TabIndex = 24;
			this.panelTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelTitle_MouseClick);
			// 
			// panelActions
			// 
			this.panelActions.Controls.Add(this.llblEditPlan);
			this.panelActions.Controls.Add(this.llblDeletePlan);
			this.panelActions.Controls.Add(this.llblRunNow);
			this.panelActions.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelActions.Location = new System.Drawing.Point(0, 128);
			this.panelActions.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.panelActions.Name = "panelActions";
			this.panelActions.Size = new System.Drawing.Size(398, 28);
			this.panelActions.TabIndex = 25;
			// 
			// panelContents
			// 
			this.panelContents.AutoSize = true;
			this.panelContents.Controls.Add(this.groupBox1);
			this.panelContents.Controls.Add(this.panelActions);
			this.panelContents.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelContents.Location = new System.Drawing.Point(8, 36);
			this.panelContents.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.panelContents.Name = "panelContents";
			this.panelContents.Size = new System.Drawing.Size(398, 156);
			this.panelContents.TabIndex = 26;
			// 
			// timer1
			// 
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// BackupPlanViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelTitle);
			this.Controls.Add(this.panelContents);
			this.Name = "BackupPlanViewControl";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.Size = new System.Drawing.Size(414, 200);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.panelTitle.ResumeLayout(false);
			this.panelTitle.PerformLayout();
			this.panelActions.ResumeLayout(false);
			this.panelActions.PerformLayout();
			this.panelContents.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel llblEditPlan;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblDuration;
		private System.Windows.Forms.Label lblFilesUploaded;
		private System.Windows.Forms.Label lblLastRun;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblSchedule;
		private System.Windows.Forms.Label lblSources;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel llblDeletePlan;
		private System.Windows.Forms.LinkLabel llblRunNow;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Panel panelTitle;
		private System.Windows.Forms.Panel panelActions;
		private System.Windows.Forms.Panel panelContents;
		private System.Windows.Forms.Timer timer1;

	}
}
