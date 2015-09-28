using Amazon;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			// This is `true` because it should dispose the `StorageBackend` implementation passed to the base class.
			_shouldDispose = true;
			PathBuilder = new S3PathBuilder();
		}

		public override async Task UploadVersionedFile(string sourcePath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string targetPath = PathBuilder.BuildVersionedRemotePath(sourcePath, version);
			await UploadFile(sourcePath, targetPath, userData);
		}

		public override async Task UploadFile(string sourcePath, string targetPath, object userData)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.UploadFile(sourcePath, targetPath, userData, this.CancellationTokenSource.Token);
			});
		}

		public override async Task DownloadVersionedFile(string targetPath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string sourcePath = PathBuilder.BuildVersionedRemotePath(targetPath, version);
			await DownloadFile(targetPath, sourcePath, userData);
		}

		public override async Task DownloadFile(string targetPath, string sourcePath, object userData)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.DownloadFile(targetPath, sourcePath, userData, this.CancellationTokenSource.Token);
			});
		}

		public override async Task List(string prefix, bool recursive, object userData)
		{
			await ExecuteOnBackround(() =>
			{
				Implementation.List(prefix, recursive, userData, this.CancellationTokenSource.Token);
			});
		}

		public override async Task DeleteVersionedFile(string sourcePath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string targetPath = PathBuilder.BuildVersionedRemotePath(sourcePath, version);

			await ExecuteOnBackround(() =>
			{
				Implementation.DeleteFile(targetPath, userData, this.CancellationTokenSource.Token);
			});
		}

		private Tuple<string /*sourcePath*/, object /*userData*/> ConvertToTarget(Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/> source)
		{
			string targetPath = PathBuilder.BuildVersionedRemotePath(source.Item1, source.Item2);
			object userData  = source.Item3;
			return new Tuple<string, object>(targetPath, userData);
		}

		public override async Task DeleteMultipleVersionedFile(List<Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/>> files)
		{
			Debug.Assert(PathBuilder != null);
			List<Tuple<string, object>> targetPaths = (from f in files select ConvertToTarget(f)).ToList();

			await ExecuteOnBackround(() =>
			{
				Implementation.DeleteMultipleFiles(targetPaths, this.CancellationTokenSource.Token);
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
