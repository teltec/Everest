using System;
using Teltec.Common;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Models
{
    public class AmazonS3Account : ObservableObject, ICloudStorageAccount
    {
        private Guid _Id;
        public Guid Id
        {
            get { return _Id; }
            set { SetField(ref _Id, value); }
        }

        public const int DisplayNameMaxLen = 16;
        private String _DisplayName;
        public String DisplayName
        {
            get { return _DisplayName; }
            set { SetField(ref _DisplayName, value); }
        }

        // http://docs.aws.amazon.com/IAM/latest/APIReference/API_AccessKey.html
        public const int AccessKeyNameMinLen = 16;
        public const int AccessKeyNameMaxLen = 32;
        private String _AccessKey;
        public String AccessKey
        {
            get { return _AccessKey; }
            set { SetField(ref _AccessKey, value); }
        }

        private String _SecretKey;
        public String SecretKey
        {
            get { return _SecretKey; }
            set { SetField(ref _SecretKey, value); }
        }

        // http://docs.aws.amazon.com/AmazonS3/latest/dev/BucketRestrictions.html
        public const int BucketNameMinLen = 3;
        public const int BucketNameMaxLen = 63;
        private String _BucketName;
        public String BucketName
        {
            get { return _BucketName; }
            set { SetField(ref _BucketName, value); }
        }
    }
}
