using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Windows.Forms;
using Teltec.Backup.Models;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Forms.S3
{
    public partial class AmazonS3AccountForm : Form
    {
        public event AccountSavedEventHandler AccountSaved;
        public event AccountCancelledEventHandler AccountCancelled;
        public AmazonS3Account _account;

        public AmazonS3AccountForm(AmazonS3Account account)
        {
            InitializeComponent();
            _account = account;
            // Setup data bindings
            tbDisplayName.DataBindings.Add(new Binding("Text", _account, this.GetPropertyName((AmazonS3Account x) => x.DisplayName), false, DataSourceUpdateMode.OnPropertyChanged));
            tbAccessKey.DataBindings.Add(new Binding("Text", _account, this.GetPropertyName((AmazonS3Account x) => x.AccessKey), false, DataSourceUpdateMode.OnPropertyChanged));
            tbSecretKey.DataBindings.Add(new Binding("Text", _account, this.GetPropertyName((AmazonS3Account x) => x.SecretKey), false, DataSourceUpdateMode.OnPropertyChanged));
            cbBucketName.DataBindings.Add(new Binding("Text", _account, this.GetPropertyName((AmazonS3Account x) => x.BucketName), false, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (AccountCancelled != null)
                AccountCancelled(this, new AmazonS3AccountSaveEventArgs(_account));
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CleanData();

            if (!IsValid())
            {
                MessageBox.Show("Invalid data");
                return;
            }

            if (AccountSaved != null)
                AccountSaved(this, new AmazonS3AccountSaveEventArgs(_account));
            this.Close();
        }

        private void CleanData()
        {
            _account.BucketName = _account.BucketName.ToLower();
        }

        private bool IsValid()
        {
            return true;
        }

        private bool HasValidCredentials()
        {
            bool hasValidAccessKey = _account.AccessKey.Length >= AmazonS3Account.ACCESS_KEY_NAME_MIN_LEN
                && _account.AccessKey.Length <= AmazonS3Account.ACCESS_KEY_NAME_MAX_LEN;
            bool hasValidSecretKey = _account.SecretKey.Length > 0;
            
            return hasValidAccessKey && hasValidSecretKey;
        }

        private void cbBucketName_DropDown(object sender, EventArgs e)
        {
            if (!HasValidCredentials())
                return;

            Amazon.RegionEndpoint region = Amazon.RegionEndpoint.SAEast1;
            AmazonS3Client client = new AmazonS3Client(_account.AccessKey, _account.SecretKey, region);
            ListBucketsResponse response = client.ListBuckets();

            Console.WriteLine("Found {0} buckets", response.Buckets.Count);
            bool found = response.Buckets.Count > 0;
            if (found) { 
                cbBucketName.Items.Clear();

                cbBucketName.Items.Add("<Create new bucket>");

                foreach (var bucket in response.Buckets)
                {
                    cbBucketName.Items.Add(bucket.BucketName);
                }
            }
        }

        private void cbBucketName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBucketName.SelectedIndex == 0) {
                MessageBox.Show("Show <Create new bucket> window.");
            }
        }
    }

    public delegate void AccountSavedEventHandler(object sender, AmazonS3AccountSaveEventArgs e);
    public delegate void AccountCancelledEventHandler(object sender, AmazonS3AccountSaveEventArgs e);

    public class AmazonS3AccountSaveEventArgs : EventArgs
    {
        private AmazonS3Account _account;
        public AmazonS3Account Account
        {
            get { return _account; }
        }

        public AmazonS3AccountSaveEventArgs(AmazonS3Account account) {
            _account = account;
        }
    }
}
