using Amazon;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;
using Teltec.Storage.Agent;

namespace Teltec.Storage.Implementations.S3
{
	public sealed class S3AsyncTransferAgent : AsyncTransferAgent
	{
		bool _shouldDispose = false;
		bool _isDisposed;

		#region Public API

		public S3AsyncTransferAgent(EventDispatcher dispatcher, AWSCredentials awsCredentials, string awsBucketName)
			: base(dispatcher, new S3StorageBackend(awsCredentials, awsBucketName, RegionEndpoint.USEast1))
		{
			_shouldDispose = true;
		}

		public override async Task UploadFile(string sourcePath)
		{
			string targetPath = BuildTargetPath(sourcePath);
			await UploadFile(sourcePath, targetPath);
		}

		public override async Task UploadFile(string sourcePath, string targetPath)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.UploadFile(sourcePath, targetPath, this.CancellationTokenSource.Token);
			});
		}

		public override async Task DownloadFile(string sourcePath)
		{
			string targetPath = "...";
			await DownloadFile(sourcePath, targetPath);
		}

		public override async Task DownloadFile(string sourcePath, string targetPath)
		{
			await ExecuteOnBackround(() =>
			{
				throw new NotImplementedException();
			});
		}

		#endregion

		#region Versioning?

		private string BuildTargetPath(string sourcePath)
		{
			string root = Path.GetPathRoot(sourcePath);
			// Remove root from Windows path, if any.
			if (root != null && root.EndsWith(@":\"))
				root = "" + root[0];
			// Remove root from Linux/Unix path, if any.
			if (sourcePath.StartsWith("/", StringComparison.Ordinal))
				sourcePath = sourcePath.Substring(1);
			// Build the key.
			string key = string.IsNullOrEmpty(RemoteRootDir)
				? root + "/" + BuildKeyPrefix(sourcePath)
				: RemoteRootDir + "/" + root + "/" + BuildKeyPrefix(sourcePath);
			return key;
		}

		private string BuildKeyPrefix(string path)
		{
			// Examples:
			//   "C:\foo\bar"       -> "foo\bar"
			//   "\\remote\foo\bar" -> "bar" (given that the network share is "\\remote\foo")
			return path.Substring(Path.GetPathRoot(path).Length).Replace('\\', '/');
		}

		#endregion

		#region Dispose Pattern Implementation

		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					if (Implementation != null)
					{
						Implementation.Dispose();
						Implementation = null;
					}
				}
				this._isDisposed = true;
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
