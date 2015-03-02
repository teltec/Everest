using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Globalization;
using System.Windows.Forms;
using Teltec.Backup.App.Models;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Forms.S3
{
	public partial class AmazonS3AccountForm : Form
	{
		public event EventHandler<AmazonS3AccountSaveEventArgs> AccountSaved;
		public event EventHandler<AmazonS3AccountSaveEventArgs> AccountCanceled;
		private AmazonS3Account _account;

		public AmazonS3AccountForm(AmazonS3Account account)
		{
			if (account.Type != EStorageAccountType.AmazonS3)
				throw new ArgumentException("Attempt to edit an account of an incompatible type");

			InitializeComponent();
			_account = account;

			// Setup data bindings
			tbDisplayName.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((AmazonS3Account x) => x.DisplayName)));
			tbAccessKey.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((AmazonS3Account x) => x.AccessKey)));
			tbSecretKey.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((AmazonS3Account x) => x.SecretKey)));
			cbBucketName.DataBindings.Add(new Binding("Text", _account,
				this.GetPropertyName((AmazonS3Account x) => x.BucketName)));
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			if (AccountCanceled != null)
				AccountCanceled(this, new AmazonS3AccountSaveEventArgs(_account));
			this.Close();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			CleanData();

			//if (!IsValid())
			//{
			//	MessageBox.Show("Invalid data");
			//	return;
			//}

			if (AccountSaved != null)
				AccountSaved(this, new AmazonS3AccountSaveEventArgs(_account));
			this.Close();
		}

		private void CleanData()
		{
			if (!String.IsNullOrEmpty(_account.BucketName))
				_account.BucketName = _account.BucketName.ToLower(CultureInfo.CurrentCulture);
		}

		private bool IsValid()
		{
			bool hasValidCredentials = HasValidCredentials();
			bool didSelectBucketName = cbBucketName.SelectedIndex > 0;
			return hasValidCredentials && didSelectBucketName;
		}

		private bool HasValidCredentials()
		{
			bool hasAccessKey = _account.AccessKey != null;
			bool hasValidAccessKey = hasAccessKey
				&& _account.AccessKey.Length >= AmazonS3Account.AccessKeyNameMinLen
				&& _account.AccessKey.Length <= AmazonS3Account.AccessKeyNameMaxLen;
			bool hasSecretKey = _account.SecretKey != null;
			bool hasValidSecretKey = hasSecretKey && _account.SecretKey.Length > 0;

			return hasValidAccessKey && hasValidSecretKey;
		}

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
				MessageBox.Show("Show <Create new bucket> window.");
			}
		}
	}

	public class AmazonS3AccountSaveEventArgs : EventArgs
	{
		private AmazonS3Account _account;
		public AmazonS3Account Account
		{
			get { return _account; }
		}

		public AmazonS3AccountSaveEventArgs(AmazonS3Account account)
		{
			_account = account;
		}
	}
}
