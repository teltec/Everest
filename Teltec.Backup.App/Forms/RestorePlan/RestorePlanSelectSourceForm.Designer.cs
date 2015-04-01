using Teltec.Backup.App.Controls;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	partial class RestorePlanSelectSourceForm
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
			this.tvFiles = new BackupPlanTreeView();
			this.loadingPanel = new Teltec.Common.Controls.SemiTransparentPanel();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Size = new System.Drawing.Size(206, 13);
			this.label1.Text = "Select files and folders you want to restore";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.loadingPanel);
			this.groupBox1.Controls.Add(this.tvFiles);
			// 
			// tvFiles
			// 
			this.tvFiles.AutoExpandMixedNodes = true;
			this.tvFiles.CheckBoxes = true;
			this.tvFiles.CheckedDataSource = null;
			this.tvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvFiles.Location = new System.Drawing.Point(5, 18);
			this.tvFiles.Name = "tvFiles";
			this.tvFiles.Size = new System.Drawing.Size(409, 283);
			this.tvFiles.TabIndex = 0;
			// 
			// loadingPanel
			// 
			this.loadingPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.loadingPanel.Location = new System.Drawing.Point(8, 158);
			this.loadingPanel.Name = "loadingPanel";
			this.loadingPanel.Size = new System.Drawing.Size(226, 140);
			this.loadingPanel.TabIndex = 1;
			this.loadingPanel.Visible = false;
			// 
			// RestorePlanSelectSourceForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "RestorePlanSelectSourceForm";
			this.Text = "Restore Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BackupPlanTreeView tvFiles;
		private Teltec.Common.Controls.SemiTransparentPanel loadingPanel;
	}
}