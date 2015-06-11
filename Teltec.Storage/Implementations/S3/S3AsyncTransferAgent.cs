using Amazon;
using Amazon.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Implementations.S3
{
	public sealed class S3AsyncTransferAgent : AsyncTransferAgent
	{
		bool _shouldDispose = false;
		bool _isDisposed;

		#region Public API

		public S3AsyncTransferAgent(AWSCredentials awsCredentials, string awsBucketName)
			: base(new S3StorageBackend(awsCredentials, awsBucketName, RegionEndpoint.USEast1))
		{
			_shouldDispose = true;
			PathBuilder = new S3PathBuilder();
		}

		public override async Task UploadVersionedFile(string sourcePath, IFileVersion version)
		{
			Debug.Assert(PathBuilder != null);
			string targetPath = PathBuilder.BuildVersionedRemotePath(sourcePath, version);
			await UploadFile(sourcePath, targetPath);
		}

		public override async Task UploadFile(string sourcePath, string targetPath)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.UploadFile(sourcePath, targetPath, this.CancellationTokenSource.Token);
			});
		}

		public override async Task DownloadVersionedFile(string targetPath, IFileVersion version)
		{
			Debug.Assert(PathBuilder != null);
			string sourcePath = PathBuilder.BuildVersionedRemotePath(targetPath, version);
			await DownloadFile(targetPath, sourcePath);
		}

		public override async Task DownloadFile(string targetPath, string sourcePath)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.DownloadFile(targetPath, sourcePath, this.CancellationTokenSource.Token);
			});
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
