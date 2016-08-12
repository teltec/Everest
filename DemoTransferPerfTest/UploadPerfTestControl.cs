using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Common.Utils;
using Teltec.FileSystem;
using Teltec.Stats;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;

namespace DemoTransferPerformance
{
	public partial class UploadPerfTestControl : UserControl
	{
		public UploadPerfTestControl()
		{
			InitializeComponent();
		}

		private void btnOpenFileDialog_Click(object sender, EventArgs e)
		{
			DialogResult dr = openFileDialog1.ShowDialog(this);
			if (dr == DialogResult.OK)
				txtFilePath.Text = openFileDialog1.FileName;
		}

		private bool IsRunning = false;
		private CancellationTokenSource CancellationTokenSource;

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (IsRunning)
				return;

			IsRunning = true;

			CancellationTokenSource = new CancellationTokenSource();

			var options = new TransferAgentOptions
			{
				UploadChunkSizeInBytes = 1 * 1024 * 1024,
			};

			string accessKey = txtAccessKey.Text.Trim();
			string secretKey = txtSecretKey.Text.Trim();
			string bucketName = txtBucketName.Text.Trim();
			BasicAWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
			string localFilePath = txtFilePath.Text;
			bool fileInformed = !string.IsNullOrEmpty(localFilePath);
			bool fileExists = fileInformed && FileManager.FileExists(localFilePath);

			if (!fileInformed || !fileExists)
			{
				string message = "";
				if (!fileInformed)
					message = "You have to inform a file for upload";
				else if (!fileExists)
					message = string.Format("The informed file does not exist: {0}", localFilePath);
				MessageBox.Show(message, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				IsRunning = false;
				return;
			}

#if true
			string remoteFilePath = typeof(UploadPerfTestControl).Name + ".DELETE_ME";
#else
			S3PathBuilder pathBuilder = new S3PathBuilder();
			string remoteFilePath = pathBuilder.BuildRemotePath(localFilePath);
#endif
			long fileSize = FileManager.UnsafeGetFileSize(localFilePath);
			BlockPerfStats stats = new BlockPerfStats();

			S3TransferAgent xferAgent = new S3TransferAgent(options, credentials, bucketName, CancellationTokenSource.Token);
			xferAgent.UploadFileStarted += (object sender1, TransferFileProgressArgs e1) =>
			{
				stats.Begin();
			};
			xferAgent.UploadFileCanceled += (object sender1, TransferFileProgressArgs e1) =>
			{
				stats.End();
				string message = "Canceled file upload";
				MessageBox.Show(message, "Transfer canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
			};
			xferAgent.UploadFileFailed += (object sender1, TransferFileProgressArgs e1) =>
			{
				stats.End();
				string message = string.Format("Failed to upload file: {0}\n{1}", e1.Exception.GetType().Name, e1.Exception.Message);
				MessageBox.Show(message, "Transfer failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			};
			xferAgent.UploadFileCompleted += (object sender1, TransferFileProgressArgs e1) =>
			{
				stats.End();
				string message = string.Format(
				"Took {0} to upload {1}",
					TimeSpanUtils.GetReadableTimespan(stats.Duration),
					FileSizeUtils.FileSizeToString(fileSize)
				);
				MessageBox.Show(message, "Transfer completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
			};
			//xferAgent.UploadFileProgress += (object sender1, TransferFileProgressArgs e1) =>
			//{
			//	// ...
			//};

			xferAgent.UploadFile(localFilePath, remoteFilePath, null);

			IsRunning = false;
		}

		#region Dispose Pattern Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				CancellationTokenSource.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
