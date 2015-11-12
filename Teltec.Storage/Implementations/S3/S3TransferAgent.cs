using Amazon;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Implementations.S3
{
	public sealed class S3TransferAgent : TransferAgent
	{
		bool _shouldDispose = false;
		bool _isDisposed;

		#region Public API

		public S3TransferAgent(AWSCredentials awsCredentials, string awsBucketName, CancellationToken cancellationToken)
			: base(new S3StorageBackend(awsCredentials, awsBucketName, RegionEndpoint.USEast1), cancellationToken)
		{
			// This is `true` because it should dispose the `StorageBackend` implementation passed to the base class.
			_shouldDispose = true;
			PathBuilder = new S3PathBuilder();
		}

		public override void UploadVersionedFile(string sourcePath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string targetPath = PathBuilder.BuildVersionedRemotePath(sourcePath, version);
			UploadFile(sourcePath, targetPath, userData);
		}

		public override void UploadFile(string sourcePath, string targetPath, object userData)
		{
			Implementation.UploadFile(sourcePath, targetPath, userData, this.CancellationToken);
		}

		public override void DownloadVersionedFile(string targetPath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string sourcePath = PathBuilder.BuildVersionedRemotePath(targetPath, version);
			DownloadFile(targetPath, sourcePath, userData);
		}

		public override void DownloadFile(string targetPath, string sourcePath, object userData)
		{
			Implementation.DownloadFile(targetPath, sourcePath, userData, this.CancellationToken);
		}

		public override void List(string prefix, bool recursive, object userData)
		{
			Implementation.List(prefix, recursive, userData, this.CancellationToken);
		}

		public override void DeleteVersionedFile(string sourcePath, IFileVersion version, object userData)
		{
			Debug.Assert(PathBuilder != null);
			string targetPath = PathBuilder.BuildVersionedRemotePath(sourcePath, version);

			Implementation.DeleteFile(targetPath, userData, this.CancellationToken);
		}

		private Tuple<string /*sourcePath*/, object /*userData*/> ConvertToTarget(Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/> source)
		{
			string targetPath = PathBuilder.BuildVersionedRemotePath(source.Item1, source.Item2);
			object userData  = source.Item3;
			return new Tuple<string, object>(targetPath, userData);
		}

		public override void DeleteMultipleVersionedFile(List<Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/>> files)
		{
			Debug.Assert(PathBuilder != null);
			List<Tuple<string, object>> targetPaths = (from f in files select ConvertToTarget(f)).ToList();

			Implementation.DeleteMultipleFiles(targetPaths, this.CancellationToken);
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
