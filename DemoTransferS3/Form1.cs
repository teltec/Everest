/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;

namespace DemoTransferS3
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			Info("Initialized.");

			txtSourceDirectory.RootFolder = Environment.SpecialFolder.MyComputer;

			LoadSettings();
		}

		private static readonly string SettingsFileName = "settings.properties";
		private Dictionary<string, string> Settings = new Dictionary<string, string>();

		private string GetSetting(string key)
		{
			string value;
			Settings.TryGetValue(key, out value);
			return value;
		}

		private void SetSetting(string key, string value)
		{
			Settings[key] = value;
		}

		public void LoadSettings()
		{
			bool loaded = Settings.ReadFromFile(SettingsFileName);

			if (loaded)
			{
				tbAccessKey.Text = GetSetting("AccessKey");
				tbSecretKey.Text = GetSetting("SecretKey");
				tbBucketName.Text = GetSetting("BucketName");
				txtSourceDirectory.Text = GetSetting("SourceDirectory");
			}
		}

		public void SaveSettings()
		{
			SetSetting("AccessKey", tbAccessKey.Text);
			SetSetting("SecretKey", tbSecretKey.Text);
			SetSetting("BucketName", tbBucketName.Text);
			SetSetting("SourceDirectory", txtSourceDirectory.Text);

			Settings.WriteToFile(SettingsFileName);
		}

		private bool ValidateForm()
		{
			if (string.IsNullOrWhiteSpace(tbAccessKey.Text))
			{
				tbAccessKey.Focus();
				goto invalid;
			}

			if (string.IsNullOrWhiteSpace(tbSecretKey.Text))
			{
				tbSecretKey.Focus();
				goto invalid;
			}

			if (string.IsNullOrWhiteSpace(tbBucketName.Text))
			{
				tbBucketName.Focus();
				goto invalid;
			}

			if (string.IsNullOrWhiteSpace(txtSourceDirectory.Text))
			{
				txtSourceDirectory.Focus();
				goto invalid;
			}

			return true;

		invalid:
			MessageBox.Show("Please, fill all required fields.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		#region Form events

		private void btnBackup_Click(object sender, EventArgs e)
		{
			if (_Operation != OperationType.BACKUP && _Operation != OperationType.UNKNOWN)
				return;

			if (_IsRunning)
			{
				DoCancel();
				_Operation = OperationType.UNKNOWN;
				btnBackup.Text = "Backup";
			}
			else
			{
				if (!ValidateForm())
					return;
				_Operation = OperationType.BACKUP;
				btnBackup.Text = "Cancel";
				DoRun();
			}
		}

		private void btnRestore_Click(object sender, EventArgs e)
		{
			if (_Operation != OperationType.RESTORE && _Operation != OperationType.UNKNOWN)
				return;

			if (_IsRunning)
			{
				DoCancel();
				_Operation = OperationType.UNKNOWN;
				btnRestore.Text = "Restore";
			}
			else
			{
				if (!ValidateForm())
					return;
				_Operation = OperationType.RESTORE;
				btnRestore.Text = "Cancel";
				DoRun();
			}
		}

		#endregion

		#region Transfer

		private enum OperationType
		{
			UNKNOWN = 0,
			BACKUP,
			RESTORE,
		}

		bool _IsRunning = false;
		OperationType _Operation;
		ITransferAgent _TransferAgent; // IDisposable
		CustomBackupAgent _BackupAgent;
		CustomRestoreAgent _RestoreAgent;
		CancellationTokenSource CancelTokenSource;

		private async void DoRun()
		{
			_IsRunning = true;

			SaveSettings();

			AsyncHelper.SettingsMaxThreadCount = Decimal.ToInt32(nudParallelism.Value);

			if (_TransferAgent != null)
				_TransferAgent.Dispose();

			transferListControl1.ClearTransfers();

			AWSCredentials awsCredentials = new BasicAWSCredentials(tbAccessKey.Text, tbSecretKey.Text);
			TransferAgentOptions options = new TransferAgentOptions();
			if (CancelTokenSource != null)
				CancelTokenSource.Dispose();
			CancelTokenSource = new CancellationTokenSource();
			_TransferAgent = new S3TransferAgent(options, awsCredentials, tbBucketName.Text, CancelTokenSource.Token);
			_TransferAgent.RemoteRootDir = "backup-99";

			switch (_Operation)
			{
				case OperationType.BACKUP:
					{
						_BackupAgent = new CustomBackupAgent(_TransferAgent);
						_BackupAgent.Results.Monitor = transferListControl1;
						/*
						_BackupAgent.Results.Failed += (object sender, TransferFileProgressArgs args, Exception e) =>
						{
							Warn("Failed {0}", args.FilePath);
						};
						_BackupAgent.Results.Canceled += (object sender, TransferFileProgressArgs args, Exception e) =>
						{
							Warn("Canceled {0}", args.FilePath);
						};
						_BackupAgent.Results.Completed += (object sender, TransferFileProgressArgs args) =>
						{
							Info("Completed {0}", args.FilePath);
						};
						_BackupAgent.Results.Started += (object sender, TransferFileProgressArgs args) =>
						{
							Info("Started {0}", args.FilePath);
						};
						_BackupAgent.Results.Progress += (object sender, TransferFileProgressArgs args) =>
						{
							Info("Progress {0}% {1} ({2}/{3} bytes)",
								args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
						};
						*/
						LinkedList<CustomVersionedFile> sources = new LinkedList<CustomVersionedFile>();
						if (cbSimulateFailure.Checked)
							sources.AddLast(new CustomVersionedFile(@"C:\pagefile.sys"));
						DirectoryInfo dir = new DirectoryInfo(txtSourceDirectory.Text);
						if (dir != null)
						{
							foreach (FileInfo file in dir.GetFiles())
								sources.AddLast(new CustomVersionedFile(file.FullName));
						}

						_BackupAgent.Files = sources;

						Info("Estimate backup size: {0} files, {1} bytes",
							_BackupAgent.Results.Stats.Total,
							FileSizeUtils.FileSizeToString(_BackupAgent.EstimatedTransferSize));

						Task task = _BackupAgent.Start();
						try
						{
							await task;
						}
						catch (Exception ex)
						{
							if (ex.IsCancellation())
								Info(ex.Message);
							else
								Error(ex.Message);
						}
						break;
					}
				case OperationType.RESTORE:
					{
						_RestoreAgent = new CustomRestoreAgent(_TransferAgent);
						_RestoreAgent.Results.Monitor = transferListControl1;

						// TODO(jweyrich): These are statically hardcoded for now, but they should be dynamic.
						//                 To make them dynamic we need to execute a Sync operation to discover them first.
						LinkedList<CustomVersionedFile> sources = new LinkedList<CustomVersionedFile>();
						sources.AddLast(new CustomVersionedFile(@"C:\pagefile.sys"));
						sources.AddLast(new CustomVersionedFile(@"C:\Teste\a.txt"));
						sources.AddLast(new CustomVersionedFile(@"C:\Teste\b.txt"));
						sources.AddLast(new CustomVersionedFile(@"C:\Teste\bash.exe"));
						sources.AddLast(new CustomVersionedFile(@"C:\Teste\c.txt"));
						sources.AddLast(new CustomVersionedFile(@"C:\Teste\e.txt"));

						_RestoreAgent.Files = sources;

						Info("Estimate backup size: {0} files, {1} bytes",
							_RestoreAgent.Results.Stats.Total,
							FileSizeUtils.FileSizeToString(_RestoreAgent.EstimatedTransferSize));

						Task task = _RestoreAgent.Start();
						try
						{
							await task;
						}
						catch (Exception ex)
						{
							if (ex.IsCancellation())
								Info(ex.Message);
							else
								Error(ex.Message);
						}
						break;
					}
			}

			OnFinish();
		}

		private void DoCancel()
		{
			if (!_IsRunning)
				return;

			switch (_Operation)
			{
				case OperationType.BACKUP:
					_BackupAgent.Cancel();
					break;
				case OperationType.RESTORE:
					_RestoreAgent.Cancel();
					break;
			}
		}

		private void OnFinish()
		{
			_IsRunning = false;

			TransferResults.Statistics stats = null;

			switch (_Operation)
			{
				case OperationType.BACKUP:
					btnBackup.Text = "Backup";
					stats = _BackupAgent.Results.Stats;
					Info("Backup completed! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
						stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
					break;
				case OperationType.RESTORE:
					btnRestore.Text = "Restore";
					stats = _RestoreAgent.Results.Stats;
					Info("Restore completed! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
						stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
					break;
			}

			_Operation = OperationType.UNKNOWN;
		}

		#endregion

		#region Logging

		private void Log(System.Diagnostics.EventLogEntryType type, string format, params object[] args)
		{
			string message = type.ToString() + " " + string.Format(format, args);
			//Console.WriteLine(message);

			listBox1.Items.Add(message);
			listBox1.TopIndex = listBox1.Items.Count - 1;
		}

		private void Warn(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, format, args);
		}

		private void Error(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, format, args);
		}

		private void Info(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, format, args);
		}

		#endregion
	}
}
