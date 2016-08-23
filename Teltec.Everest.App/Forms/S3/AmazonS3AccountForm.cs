using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Everest.App.Forms.Account;
using Teltec.Common.Extensions;
using Teltec.Common.Types;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.S3
{
	public partial class AmazonS3AccountForm : Form
	{
		public event EventHandler<AmazonS3AccountSaveEventArgs> AccountSaved;
		public event EventHandler<AmazonS3AccountSaveEventArgs> AccountCanceled;
		private Models.AmazonS3Account _account;
		private Tribool _accountAlreadyHasBackup = Tribool.Unknown;

		public AmazonS3AccountForm(Models.AmazonS3Account account)
		{
			if (account.Type != Models.EStorageAccountType.AmazonS3)
				throw new ArgumentException("Attempt to edit an account of an incompatible type");

			InitializeComponent();
			_account = account;

			// Setup data bindings
			tbDisplayName.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((Models.AmazonS3Account x) => x.DisplayName)));
			tbAccessKey.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((Models.AmazonS3Account x) => x.AccessKey)));
			tbSecretKey.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((Models.AmazonS3Account x) => x.SecretKey)));
			cbBucketName.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((Models.AmazonS3Account x) => x.BucketName)));
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			CancellationTokenSource.Cancel();

			if (AccountCanceled != null)
				AccountCanceled(this, new AmazonS3AccountSaveEventArgs(_account));

			this.Close();
		}

		private async void btnSave_Click(object sender, EventArgs e)
		{
			CleanData();

			if (!IsValid())
			{
				MessageBox.Show("Invalid data. Please verify.");
				return;
			}

			// Check whether account already has one or more backups.
			//DisableUI();
			{
				Task<List<string>> task = RetrieveAccountConfigurations();
				await task;
				if (task.Status == TaskStatus.RanToCompletion && task.Result.Count > 0)
				{
					using (AccountConfigurationSelector form = new AccountConfigurationSelector())
					{
						form.AvailableConfigurations = task.Result;
						form.SelectedConfiguration = _account.Hostname;
						form.ShowDialog(this);

						if (!string.IsNullOrEmpty(form.SelectedConfiguration))
							_account.Hostname = form.SelectedConfiguration;
					}
				}
			}
			//EnableUI();

			if (AccountSaved != null)
				AccountSaved(this, new AmazonS3AccountSaveEventArgs(_account));
			this.Close();
		}

		private void CleanData()
		{
			if (!string.IsNullOrEmpty(_account.BucketName))
				_account.BucketName = _account.BucketName.ToLower(CultureInfo.CurrentCulture);
		}

		private bool IsValid()
		{
			bool hasValidCredentials = HasValidCredentials();
			bool didSelectBucketName = cbBucketName.SelectedIndex > 0 || !string.IsNullOrEmpty(cbBucketName.Text);
			return hasValidCredentials && didSelectBucketName;
		}

		private bool HasValidCredentials()
		{
			bool hasDisplayName = !string.IsNullOrEmpty(_account.DisplayName);
			bool hasValidDisplayName = hasDisplayName
				&& _account.DisplayName.Length <= Models.AmazonS3Account.DisplayNameMaxLen;
			bool hasAccessKey = _account.AccessKey != null;
			bool hasValidAccessKey = hasAccessKey
				&& _account.AccessKey.Length >= Models.AmazonS3Account.AccessKeyIdMinLen
				&& _account.AccessKey.Length <= Models.AmazonS3Account.AccessKeyIdMaxLen;
			bool hasSecretKey = _account.SecretKey != null;
			bool hasValidSecretKey = hasSecretKey && _account.SecretKey.Length > 0;

			return hasValidDisplayName && hasValidAccessKey && hasValidSecretKey;
		}

		// TODO(jweyrich): We could create a visual component containing the ComboBox to
		//                 abstract away the complexity of dealing with AWS S3. Idea:
		//                 - Have a property to configure S3 credentials;
		//                 - Have a property to enable/disable creation of buckets;
		//                 - Have an event to detect when the selection changed;
		private void cbBucketName_DropDown(object sender, EventArgs e)
		{
			if (!HasValidCredentials())
				return;

			Amazon.RegionEndpoint region = Amazon.RegionEndpoint.SAEast1;
			AmazonS3Client client = new AmazonS3Client(_account.AccessKey, _account.SecretKey, region);
			ListBucketsResponse response;
			try
			{
				response = client.ListBuckets();
			}
			catch (Exception ex)
			{
				cbBucketName.Items.Clear();
				MessageBox.Show(ex.Message, "Error");
				return;
			}

			Console.WriteLine("Found {0} buckets", response.Buckets.Count);
			bool found = response.Buckets.Count > 0;
			if (found)
			{
				cbBucketName.Items.Clear();

				cbBucketName.Items.Add("<Create new bucket>");

				foreach (var bucket in response.Buckets)
				{
					cbBucketName.Items.Add(bucket.BucketName);
				}
			}
		}

		private void cbBucketName_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (cbBucketName.SelectedIndex == 0)
			{
				using (var form = new AmazonS3CreateBucketForm(_account))
				{
					form.BucketCreated += (object sender1, AmazonS3CreateBucketEventArgs e1) =>
					{
						int index = cbBucketName.Items.Add(e1.Account.BucketName); // Add new bucket to the ComboBox.
						cbBucketName.SelectedIndex = index; // Select it.
					};
					form.BucketCanceled += (object sender1, AmazonS3CreateBucketEventArgs e1) =>
					{
						cbBucketName.SelectedIndex = -1; // Deselect it.
					};
					form.ShowDialog(this);
				}
			}
		}

		private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		private async Task<List<string>> RetrieveAccountConfigurations()
		{
			// Renew cancellation token if was already used.
			if (CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Dispose();
				CancellationTokenSource = new CancellationTokenSource();
			}

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(_account.AccessKey, _account.SecretKey);
			TransferAgentOptions options = new TransferAgentOptions();
			ITransferAgent transferAgent = new S3TransferAgent(options, awsCredentials, _account.BucketName, CancellationTokenSource.Token);
			transferAgent.RemoteRootDir = transferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP");

			List<string> remoteObjects = new List<string>(16); // Avoid small resizes without compromising memory.

			// Register event handlers.
			transferAgent.ListingProgress += (object sender2, ListingProgressArgs e2) =>
			{
				foreach (var obj in e2.Objects)
				{
					string[] parts = obj.Key.Split(S3PathBuilder.RemoteDirectorySeparatorChar);
					int count = parts.Length;
					if (count > 2)
					{
						remoteObjects.Add(parts[count - 2]);
					}
				}
			};
			transferAgent.ListingFailed += (object sender2, ListingProgressArgs e2) =>
			{
				//var message = string.Format("Failed: {0}", e2.Exception != null ? e2.Exception.Message : "Unknown reason");
				//logger.Warn(message);
			};

			try
			{
				Task task = Task.Run(() =>
				{
					transferAgent.List(transferAgent.RemoteRootDir, false, null);
				});

				await task;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
			}

			return remoteObjects;
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

	public class AmazonS3AccountSaveEventArgs : EventArgs
	{
		private Models.AmazonS3Account _account;
		public Models.AmazonS3Account Account
		{
			get { return _account; }
		}

		public AmazonS3AccountSaveEventArgs(Models.AmazonS3Account account)
		{
			_account = account;
		}
	}
}
