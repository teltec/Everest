using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teltec.Storage.Backend;

namespace Teltec.Storage.Implementations.S3
{
	// TODO(jweyrich):
	//	1. Calculate MD5 for the whole file, number of parts, and MD5 for each part before starting the upload.
	//	2. While uploading each part, compare its MD5 with the one that was saved in step 1.
	//	   If the MD5 for any part is different, then stop the upload operation because file has been changed.
	//	3. Allow to resume upload operation if the operation gets interrupted before completing the upload process.
	public sealed class S3StorageBackend : StorageBackend
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		IAmazonS3 _s3Client; // IDisposable
		string _awsBuckeName;
		bool _shouldDispose = false;
		bool _isDisposed;

		#region Constructors

		public S3StorageBackend(AWSCredentials awsCredentials, string awsBucketName, RegionEndpoint region)
			: this(new AmazonS3Client(awsCredentials, region), awsBucketName)
		{
			this._shouldDispose = true;
		}

		public S3StorageBackend(IAmazonS3 s3Client, string awsBucketName)
		{
			this._s3Client = s3Client;
			this._awsBuckeName = awsBucketName;
		}

		#endregion

		#region Upload methods

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/LLuploadFileDotNet.html
		public override void UploadFile(string filePath, string keyName, CancellationToken cancellationToken)
		{
			FileInfo fileInfo = null;
			long contentLength = 0;
			CancelableFileStream inputStream = null;

			TransferFileProgressArgs reusedProgressArgs = new TransferFileProgressArgs
			{
				State = TransferState.PENDING,
				TotalBytes = 0,
				TransferredBytes = 0,
				FilePath = filePath,
			};

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

			InitiateMultipartUploadResponse initResponse = null;
			
			try
			{
				// Attempt to read the file before anything else.
				fileInfo = new FileInfo(filePath);
				contentLength = reusedProgressArgs.TotalBytes = fileInfo.Length;

				// Report start - before any possible failures.
				if (UploadStarted != null)
					UploadStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				// Create the input stream to actually read the file.
				inputStream = new CancelableFileStream(filePath, FileMode.Open, FileAccess.Read, cancellationToken);

				// 1. Initialize.
				InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
				{
					BucketName = this._awsBuckeName,
					Key = keyName,
					CannedACL = S3CannedACL.Private,
					StorageClass = S3StorageClass.ReducedRedundancy,
				};

				initResponse = this._s3Client.InitiateMultipartUpload(initiateRequest);

				// Report 0% progress.
				if (UploadProgressed != null)
					UploadProgressed(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.TRANSFERRING;
					});

				// 2. Upload Parts.
				// Each part must be at least 5 MB in size, except the last part.
				// There is no size limit on the last part of your multipart upload.
				// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/API/mpUploadUploadPart.html
				const long PART_SIZE = 5 * 1024 * 1024; // 5 MB = 2^20

				long filePosition = 0;

				for (int i = 1; filePosition < contentLength; i++)
				{
					if (cancellationToken != null)
						cancellationToken.ThrowIfCancellationRequested();

					UploadPartRequest uploadRequest = new UploadPartRequest
					{
						BucketName = this._awsBuckeName,
						Key = keyName,
						UploadId = initResponse.UploadId,
						PartNumber = i,
						PartSize = PART_SIZE,//Math.Min(contentLength - filePosition, PART_SIZE),
						//FilePosition = filePosition,
						//FilePath = filePath,
						//IsLastPart = contentLength - filePosition <= PART_SIZE,
						InputStream = inputStream,
					};

					// Progress handler.
					uploadRequest.StreamTransferProgress = (object sender, StreamTransferProgressArgs args) =>
						{
							filePosition = args.TransferredBytes;

							// Report progress.
							if (UploadProgressed != null)
								UploadProgressed(reusedProgressArgs, () =>
								{
									reusedProgressArgs.TransferredBytes = filePosition;
								});
						};

					// Upload part and add response to our list.
					uploadResponses.Add(this._s3Client.UploadPart(uploadRequest));
				}

				// Step 3: complete.
				CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
				{
					BucketName = this._awsBuckeName,
					Key = keyName,
					UploadId = initResponse.UploadId,
					//PartETags = new List<PartETag>(uploadResponses)
				};
				completeRequest.AddPartETags(uploadResponses);

				CompleteMultipartUploadResponse completeUploadResponse =
					this._s3Client.CompleteMultipartUpload(completeRequest);

				// Report completion.
				if (UploadCompleted != null)
					UploadCompleted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Upload canceled.");
				if (initResponse != null)
				{
					AbortMultipartUploadRequest abortRequest = new AbortMultipartUploadRequest
					{
						BucketName = this._awsBuckeName,
						Key = keyName,
						UploadId = initResponse.UploadId
					};
					this._s3Client.AbortMultipartUpload(abortRequest);
				}

				// Report cancelation.
				if (UploadCanceled != null)
					UploadCanceled(reusedProgressArgs, exception, () =>
					{
						reusedProgressArgs.State = TransferState.CANCELED;
					});
			}
			catch (Exception exception)
			{
				logger.Warn("Exception occurred: {0}", exception.Message);
				if (initResponse != null)
				{
					AbortMultipartUploadRequest abortRequest = new AbortMultipartUploadRequest
					{
						BucketName = this._awsBuckeName,
						Key = keyName,
						UploadId = initResponse.UploadId
					};
					this._s3Client.AbortMultipartUpload(abortRequest);
				}

				// Report failure.
				if (UploadFailed != null)
					UploadFailed(reusedProgressArgs, exception, () =>
					{
						reusedProgressArgs.State = TransferState.FAILED;
					});
			}
        }

		#endregion

		#region Dispose Pattern Implementation

		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					if (_s3Client != null)
					{
						_s3Client.Dispose();
						_s3Client = null;
					}
				}
				this._isDisposed = true;
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
