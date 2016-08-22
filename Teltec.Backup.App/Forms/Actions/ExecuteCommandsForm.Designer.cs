using Teltec.Common.Controls;
namespace Teltec.Backup.App.Forms.Actions
{
	partial class ExecuteCommandsForm
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
			this.flpSupportedAccounts = new System.Windows.Forms.FlowLayoutPanel();
			this.panelItemAmazonS3 = new System.Windows.Forms.Panel();
			this.cbAbortIfFailed = new System.Windows.Forms.CheckBox();
			this.cbBeforeOperation = new System.Windows.Forms.CheckBox();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.txtFodBefore = new TextBoxSelectFileDialog();
			this.panel1 = new System.Windows.Forms.Panel();
			this.txtFodAfter = new TextBoxSelectFileDialog();
			this.cbExecuteOnlyIfSuccess = new System.Windows.Forms.CheckBox();
			this.cbAfterOperation = new System.Windows.Forms.CheckBox();
			this.panelBottom.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.panelMiddle.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flpSupportedAccounts.SuspendLayout();
			this.panelItemAmazonS3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.Size = new System.Drawing.Size(401, 13);
			this.label1.Text = "Specify which commands to be executed before and/or after the backup completes";
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this.flpSupportedAccounts);
			//
			// flpSupportedAccounts
			//
			this.flpSupportedAccounts.AutoSize = true;
			this.flpSupportedAccounts.Controls.Add(this.panelItemAmazonS3);
			this.flpSupportedAccounts.Controls.Add(this.panel1);
			this.flpSupportedAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpSupportedAccounts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flpSupportedAccounts.Location = new System.Drawing.Point(5, 18);
			this.flpSupportedAccounts.Margin = new System.Windows.Forms.Padding(0);
			this.flpSupportedAccounts.Name = "flpSupportedAccounts";
			this.flpSupportedAccounts.Size = new System.Drawing.Size(409, 283);
			this.flpSupportedAccounts.TabIndex = 1;
			//
			// panelItemAmazonS3
			//
			this.panelItemAmazonS3.Controls.Add(this.txtFodBefore);
			this.panelItemAmazonS3.Controls.Add(this.cbAbortIfFailed);
			this.panelItemAmazonS3.Controls.Add(this.cbBeforeOperation);
			this.panelItemAmazonS3.Location = new System.Drawing.Point(0, 0);
			this.panelItemAmazonS3.Margin = new System.Windows.Forms.Padding(0);
			this.panelItemAmazonS3.Name = "panelItemAmazonS3";
			this.panelItemAmazonS3.Padding = new System.Windows.Forms.Padding(5);
			this.panelItemAmazonS3.Size = new System.Drawing.Size(409, 92);
			this.panelItemAmazonS3.TabIndex = 0;
			//
			// cbAbortIfFailed
			//
			this.cbAbortIfFailed.AutoSize = true;
			this.cbAbortIfFailed.Location = new System.Drawing.Point(28, 67);
			this.cbAbortIfFailed.Name = "cbAbortIfFailed";
			this.cbAbortIfFailed.Size = new System.Drawing.Size(170, 17);
			this.cbAbortIfFailed.TabIndex = 2;
			this.cbAbortIfFailed.Text = "Abort backup if this action fails";
			this.cbAbortIfFailed.UseVisualStyleBackColor = true;
			//
			// cbBeforeOperation
			//
			this.cbBeforeOperation.AutoSize = true;
			this.cbBeforeOperation.Location = new System.Drawing.Point(8, 8);
			this.cbBeforeOperation.Name = "cbBeforeOperation";
			this.cbBeforeOperation.Size = new System.Drawing.Size(228, 17);
			this.cbBeforeOperation.TabIndex = 1;
			this.cbBeforeOperation.Text = "Execute this command before backup runs";
			this.cbBeforeOperation.UseVisualStyleBackColor = true;
			//
			// openFileDialog1
			//
			this.openFileDialog1.FileName = "openFileDialog1";
			//
			// txtFodBefore
			//
			this.txtFodBefore.Location = new System.Drawing.Point(28, 31);
			this.txtFodBefore.Name = "txtFodBefore";
			this.txtFodBefore.Size = new System.Drawing.Size(371, 30);
			this.txtFodBefore.TabIndex = 3;
			//
			// panel1
			//
			this.panel1.Controls.Add(this.txtFodAfter);
			this.panel1.Controls.Add(this.cbExecuteOnlyIfSuccess);
			this.panel1.Controls.Add(this.cbAfterOperation);
			this.panel1.Location = new System.Drawing.Point(0, 92);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(409, 92);
			this.panel1.TabIndex = 1;
			//
			// txtFodAfter
			//
			this.txtFodAfter.Location = new System.Drawing.Point(28, 31);
			this.txtFodAfter.Name = "txtFodAfter";
			this.txtFodAfter.Size = new System.Drawing.Size(371, 30);
			this.txtFodAfter.TabIndex = 3;
			//
			// cbExecuteOnlyIfSuccess
			//
			this.cbExecuteOnlyIfSuccess.AutoSize = true;
			this.cbExecuteOnlyIfSuccess.Location = new System.Drawing.Point(28, 67);
			this.cbExecuteOnlyIfSuccess.Name = "cbExecuteOnlyIfSuccess";
			this.cbExecuteOnlyIfSuccess.Size = new System.Drawing.Size(331, 17);
			this.cbExecuteOnlyIfSuccess.TabIndex = 2;
			this.cbExecuteOnlyIfSuccess.Text = "Execute this command only if backup did complete without errors";
			this.cbExecuteOnlyIfSuccess.UseVisualStyleBackColor = true;
			//
			// cbAfterOperation
			//
			this.cbAfterOperation.AutoSize = true;
			this.cbAfterOperation.Location = new System.Drawing.Point(8, 8);
			this.cbAfterOperation.Name = "cbAfterOperation";
			this.cbAfterOperation.Size = new System.Drawing.Size(219, 17);
			this.cbAfterOperation.TabIndex = 1;
			this.cbAfterOperation.Text = "Execute this command after backup runs";
			this.cbAfterOperation.UseVisualStyleBackColor = true;
			//
			// ExecuteCommandsForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(634, 461);
			this.Name = "ExecuteCommandsForm";
			this.Text = "Backup Plan";
			this.panelBottom.ResumeLayout(false);
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelMiddle.ResumeLayout(false);
			this.panelMiddle.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.flpSupportedAccounts.ResumeLayout(false);
			this.panelItemAmazonS3.ResumeLayout(false);
			this.panelItemAmazonS3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.FlowLayoutPanel flpSupportedAccounts;
		protected System.Windows.Forms.Panel panelItemAmazonS3;
		private System.Windows.Forms.CheckBox cbBeforeOperation;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.CheckBox cbAbortIfFailed;
		private TextBoxSelectFileDialog txtFodBefore;
		protected System.Windows.Forms.Panel panel1;
		private TextBoxSelectFileDialog txtFodAfter;
		private System.Windows.Forms.CheckBox cbExecuteOnlyIfSuccess;
		private System.Windows.Forms.CheckBox cbAfterOperation;
    }
}
