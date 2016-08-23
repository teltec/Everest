using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.S3
{
	public partial class AmazonS3CreateBucketForm : Form
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<AmazonS3CreateBucketEventArgs> BucketCreated;
		public event EventHandler<AmazonS3CreateBucketEventArgs> BucketCanceled;
		private Models.AmazonS3Account _account;

		public AmazonS3CreateBucketForm(Models.AmazonS3Account account)
		{
			if (account.Type != Models.EStorageAccountType.AmazonS3)
				throw new ArgumentException("Attempt to create a bucket using an account with an incompatible type");

			InitializeComponent();

			LoadBucketRegions();
			LoadStorageClasses();

			_account = account;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			//CancellationTokenSource.Cancel();

			if (BucketCanceled != null)
				BucketCanceled(this, new AmazonS3CreateBucketEventArgs(_account));

			this.Close();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			CleanData();

			if (!IsValid())
			{
				MessageBox.Show("Invalid data. Please verify.");
				return;
			}

			string bucketName = tbBucketName.Text;
			Amazon.RegionEndpoint bucketRegion = cbBucketLocation.SelectedItem as Amazon.RegionEndpoint;
			Amazon.S3.S3StorageClass storageClass = cbStorageClass.SelectedItem as S3StorageClass;

			string statusMessage;
			bool created = CreateBucket(bucketName, bucketRegion, storageClass, out statusMessage);
			if (!created)
			{
				// Bucket creation failed.
				logger.Warn(statusMessage);
				MessageBox.Show(statusMessage, "Bucket creation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			else
			{
				MessageBox.Show(statusMessage, "Bucket was created", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			_account.BucketName = bucketName;

			if (BucketCreated != null)
				BucketCreated(this, new AmazonS3CreateBucketEventArgs(_account));
			this.Close();
		}

		//private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		private bool CreateBucket(string bucketName, Amazon.RegionEndpoint bucketRegion, S3StorageClass storageClass, out string statusMessage)
		{
			AWSCredentials awsCredentials = new BasicAWSCredentials(_account.AccessKey, _account.SecretKey);
			//TransferAgentOptions options = new TransferAgentOptions();
			//ITransferAgent transferAgent = new S3TransferAgent(options, awsCredentials, _account.BucketName, CancellationTokenSource.Token);

			using (var client = new AmazonS3Client(awsCredentials, bucketRegion))
			{
				bool alreadyExists = S3_DoesBucketExist(client, bucketName);
				if (alreadyExists)
				{
					statusMessage = "The informed bucket already exists. Please, pick another name.";
					return false;
				}

				try
				{
					PutBucketRequest putRequest = new PutBucketRequest
					{
						BucketName = bucketName,
						UseClientRegion = true,
					};

					PutBucketResponse response = client.PutBucket(putRequest);

					switch (response.HttpStatusCode)
					{
						default:
							statusMessage = "Unknown problem.";
							break;
						case HttpStatusCode.OK:
							statusMessage = "The bucket has been successfuly created.";
							return true;
						case HttpStatusCode.Forbidden:
							statusMessage = string.Format("Bucket creation failed: {0}", response.ToString());
							break;
					}
				}
				catch (Exception exception)
				{
					if (exception is AmazonS3Exception)
					{
						AmazonS3Exception amznException = exception as AmazonS3Exception;
						if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
						{
							statusMessage = "Check the provided AWS Credentials.";
						}
						else
						{
							statusMessage = string.Format("Error occurred. Message:'{0}' when writing an object", amznException.Message);
						}
					}
					else
					{
						statusMessage = string.Format("Exception occurred: {0}", exception.Message);
					}
				}
			}

			return false;
		}

		private bool S3_DoesBucketExist(AmazonS3Client client, string bucketName)
		{
			return AmazonS3Util.DoesS3BucketExist(client, bucketName);
		}

		//private string S3_FindBucketLocation(AmazonS3Client client, string bucketName)
		//{
		//	string bucketLocation;
		//	GetBucketLocationRequest request = new GetBucketLocationRequest()
		//	{
		//		BucketName = bucketName
		//	};
		//	GetBucketLocationResponse response = client.GetBucketLocation(request);
		//	bucketLocation = response.Location.ToString();
		//	return bucketLocation;
		//}

		private void CleanData()
		{
			// TODO: Does the SKD support bucket name validation/sanitization?
			// See http://docs.aws.amazon.com/AmazonS3/latest/dev/BucketRestrictions.html
			if (!string.IsNullOrEmpty(tbBucketName.Text))
				tbBucketName.Text = tbBucketName.Text.ToLower(CultureInfo.CurrentCulture);
		}

		private bool IsValid()
		{
			bool hasBucketName = !string.IsNullOrEmpty(tbBucketName.Text);
			bool didSelectBucketLocation = cbBucketLocation.SelectedIndex > 0 || !string.IsNullOrEmpty(cbBucketLocation.Text);
			bool didSelectStorageClass = cbStorageClass.SelectedIndex > 0 || !string.IsNullOrEmpty(cbStorageClass.Text);
			return hasBucketName && didSelectBucketLocation && didSelectStorageClass;
		}

		private void LoadBucketRegions()
		{
			if (cbBucketLocation.Items.Count > 0)
				return;

			IEnumerable<Amazon.RegionEndpoint> regions = Amazon.RegionEndpoint.EnumerableAllRegions;
			cbBucketLocation.Items.AddRange(regions.ToArray());

			//cbBucketLocation.DisplayMember = this.GetPropertyName((Amazon.RegionEndpoint x) => x.DisplayName);
			//cbBucketLocation.ValueMember = this.GetPropertyName((Amazon.RegionEndpoint x) => x.SystemName);

			if (cbBucketLocation.Items.Count > 0)
				cbBucketLocation.SelectedIndex = 0;
		}

		private void cbBucketLocation_DropDown(object sender, EventArgs e)
		{
			// ...
		}

		private void LoadStorageClasses()
		{
			if (cbStorageClass.Items.Count > 0)
				return;

			List<S3StorageClass> classes = new List<S3StorageClass>(4);
			classes.Add(S3StorageClass.Standard);
			classes.Add(S3StorageClass.StandardInfrequentAccess);
			classes.Add(S3StorageClass.Glacier);
			classes.Add(S3StorageClass.ReducedRedundancy);
			cbStorageClass.Items.AddRange(classes.ToArray());

			// If the specified property does not exist on the object or the value
			// of DisplayMember is an empty string (""), the results of the object's
			// ToString method are displayed instead.
			//cbStorageClass.DisplayMember = "";
			//cbStorageClass.ValueMember = "";

			if (cbStorageClass.Items.Count > 0)
				cbStorageClass.SelectedIndex = 0;
		}

		private void cbStorageClass_DropDown(object sender, EventArgs e)
		{
			// ...
		}

		private void cbBucketLocation_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// ...
		}

		private void cbStorageClass_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// ...
		}
	}

	public class AmazonS3CreateBucketEventArgs : EventArgs
	{
		private Models.AmazonS3Account _account;
		public Models.AmazonS3Account Account
		{
			get { return _account; }
		}

		public AmazonS3CreateBucketEventArgs(Models.AmazonS3Account account)
		{
			_account = account;
		}
	}
}
