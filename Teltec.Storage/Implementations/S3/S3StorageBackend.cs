using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

		#region Upload

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/LLuploadFileDotNet.html
		public override void UploadFile(string filePath, string keyName, CancellationToken cancellationToken)
		{
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
				ZetaLongPaths.ZlpFileInfo fileInfo = new ZetaLongPaths.ZlpFileInfo(filePath);
				long contentLength = reusedProgressArgs.TotalBytes = fileInfo.Length;

				// Report start - before any possible failures.
				if (UploadStarted != null)
					UploadStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				// Create the input stream to actually read the file.
				inputStream = new CancelableFileStream(filePath, FileMode.Open, FileAccess.Read, cancellationToken);

				// Step 1: Initialize.
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

				// Step 2: Upload Parts.
				// Each part must be at least 5 MB in size, except the last part.
				// There is no size limit on the last part of your multipart upload.
				// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/API/mpUploadUploadPart.html
				const long PART_SIZE = 5 * 1024 * 1024; // 5 MB = 2^20

				long filePosition = 0;

				long partTotal = (long)Math.Ceiling((decimal)contentLength / PART_SIZE);

				for (int partNumber = 1; partNumber <= partTotal; partNumber++)
				{
					if (cancellationToken != null)
						cancellationToken.ThrowIfCancellationRequested();

					UploadPartRequest uploadRequest = new UploadPartRequest
					{
						BucketName = this._awsBuckeName,
						Key = keyName,
						UploadId = initResponse.UploadId,
						PartNumber = partNumber,
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
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Warn("Check the provided AWS Credentials.");
					}
					else
					{
						logger.Warn("Error occurred. Message:'{0}' when uploading object", amznException.Message);
					}
				}
				else
				{
					logger.Warn("Exception occurred: {0}", exception.Message);
				}

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
			finally
			{
				if (inputStream != null)
				{
					inputStream.Close();
					inputStream.Dispose();
				}
			}
        }

		#endregion

		#region Download

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html
		public override void DownloadFile(string filePath, string keyName, CancellationToken cancellationToken)
		{
			TransferFileProgressArgs reusedProgressArgs = new TransferFileProgressArgs
			{
				State = TransferState.PENDING,
				TotalBytes = 0,
				TransferredBytes = 0,
				FilePath = filePath,
			};

			// Download request.
			GetObjectRequest downloadRequest = new GetObjectRequest
			{
				BucketName = this._awsBuckeName,
				Key = keyName,
			};

			try
			{
				// Attempt to create any intermediary directories before anything else.
				ZetaLongPaths.ZlpFileInfo file = new ZetaLongPaths.ZlpFileInfo(filePath);
				ZetaLongPaths.ZlpIOHelper.CreateDirectory(file.DirectoryName);

				// Report start - before any possible failures.
				if (DownloadStarted != null)
					DownloadStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				const int DefaultBufferSize = 8192;

				// REFERENCE: https://github.com/aws/aws-sdk-net/blob/5f19301ee9fa1ec29b11b3dfdee82071a04ed5ae/AWSSDK_DotNet35/Amazon.S3/Model/GetObjectResponse.cs
				// Download.
				// Create the file. If the file already exists, it will be overwritten.
				using (GetObjectResponse downloadResponse = this._s3Client.GetObject(downloadRequest))
				using (BufferedStream bufferedStream = new BufferedStream(downloadResponse.ResponseStream))
				using (Stream fileStream = new BufferedStream(new FileStream(filePath, FileMode.Create)))
				{
					// Report 0% progress.
					if (DownloadProgressed != null)
						DownloadProgressed(reusedProgressArgs, () =>
						{
							reusedProgressArgs.TotalBytes = downloadResponse.ContentLength;
							reusedProgressArgs.State = TransferState.TRANSFERRING;
						});

					string requestId = downloadResponse.ResponseMetadata.RequestId;
					string amzId2;
					downloadResponse.ResponseMetadata.Metadata.TryGetValue(HeaderKeys.XAmzId2Header, out amzId2);
					amzId2 = amzId2 ?? string.Empty;

					long filePosition = 0;
					int bytesRead = 0;
					byte[] buffer = new byte[DefaultBufferSize];
					while ((bytesRead = bufferedStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (cancellationToken != null)
							cancellationToken.ThrowIfCancellationRequested();

						fileStream.Write(buffer, 0, bytesRead);
						filePosition += bytesRead;

						// Report progress.
						if (DownloadProgressed != null)
							DownloadProgressed(reusedProgressArgs, () =>
							{
								reusedProgressArgs.TransferredBytes = filePosition;
							});
					}

					// Validate transferred size.
					if (reusedProgressArgs.TransferredBytes != reusedProgressArgs.TotalBytes)
					{
						var message = string.Format(CultureInfo.InvariantCulture,
							"The total bytes read {0} from response stream is not equal to the Content-Length {1} for the object {2} in bucket {3}."
							+ " Request ID = {4} , AmzId2 = {5}.",
							reusedProgressArgs.TransferredBytes,
							reusedProgressArgs.TotalBytes,
							keyName, this._awsBuckeName, requestId, amzId2);

						throw new StreamSizeMismatchException(message, reusedProgressArgs.TotalBytes, reusedProgressArgs.TransferredBytes, requestId, amzId2);
					}
				}

				// Report completion.
				if (DownloadCompleted != null)
					DownloadCompleted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Download canceled.");

				// Report cancelation.
				if (DownloadCanceled != null)
					DownloadCanceled(reusedProgressArgs, exception, () =>
					{
						reusedProgressArgs.State = TransferState.CANCELED;
					});
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Warn("Check the provided AWS Credentials.");
					}
					else
					{
						logger.Warn("Error occurred. Message:'{0}' when downloading object", amznException.Message);
					}
				}
				else
				{
					logger.Warn("Exception occurred: {0}", exception.Message);
				}

				// Report failure.
				if (DownloadFailed != null)
					DownloadFailed(reusedProgressArgs, exception, () =>
					{
						reusedProgressArgs.State = TransferState.FAILED;
					});
			}
		}

		#endregion

		#region Listing

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingObjectKeysUsingNetSDK.html
		public override void List(string prefix, bool recursive, CancellationToken cancellationToken)
		{
			ListingProgressArgs reusedProgressArgs = new ListingProgressArgs
			{
				State = TransferState.PENDING,
				Objects = new List<ListingObject>(),
			};

			// List request.
			ListObjectsRequest listRequest = new ListObjectsRequest
			{
				BucketName = this._awsBuckeName,

			};

			if (!string.IsNullOrEmpty(prefix))
				listRequest.Prefix = prefix;

			// Listing Keys Hierarchically Using a Prefix and Delimiter
			// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
			if (!recursive)
				listRequest.Delimiter = S3PathBuilder.RemoteDirectorySeparatorChar.ToString();

			try
			{
				// Report start - before any possible failures.
				if (ListingStarted != null)
					ListingStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				// Report 0% progress.
				if (ListingProgressed != null)
					ListingProgressed(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.TRANSFERRING;
					});

				// Listing.
				do
				{
					if (cancellationToken != null)
						cancellationToken.ThrowIfCancellationRequested();

					ListObjectsResponse response = this._s3Client.ListObjects(listRequest);

					//if (!recursive)
					//{
					//	// Get the TOP LEVEL objects - those not inside any folders.
					//	resultObjects = resultObjects.Where(o => !o.Key.Contains(@"/"));
					//
					//	// Get the folders at the TOP LEVEL only
					//	//var topLevelFolders = resultObjects.Except(objects).Where(o => o.Key.Last() == '/' && o.Key.IndexOf(@"/") == o.Key.LastIndexOf(@"/"));
					//}

					if (!recursive)
					{
						IEnumerable<string> commonPrefixes = response.CommonPrefixes;

						// Process response.
						foreach (string key in commonPrefixes)
						{
							Console.WriteLine("Key = {0}", key);
						}

						// If response is truncated, set the marker to get the next set of keys.
						if (response.IsTruncated)
						{
							listRequest.Marker = response.NextMarker;
						}
						else
						{
							listRequest = null;
						}

						// Report progress.
						if (ListingProgressed != null)
						{
							var queryResults =
								from key in commonPrefixes
								select new ListingObject
								{
									ETag = null,
									Key = key,
									LastModified = null,
									Size = 0,
								};
							reusedProgressArgs.Objects.Clear();
							reusedProgressArgs.Objects.AddRange(queryResults);
							ListingProgressed(reusedProgressArgs, () =>
							{
							});
						}
					}
					else
					{
						IEnumerable<S3Object> resultObjects = response.S3Objects;

						// Process response.
						foreach (S3Object entry in resultObjects)
						{
							Console.WriteLine("Key = {0}, Size = {1}, LastModified = {2}", entry.Key, entry.Size, entry.LastModified);
						}

						// If response is truncated, set the marker to get the next set of keys.
						if (response.IsTruncated)
						{
							listRequest.Marker = response.NextMarker;
						}
						else
						{
							listRequest = null;
						}

						// Report progress.
						if (ListingProgressed != null)
						{
							var queryResults =
								from obj in resultObjects
								select new ListingObject
								{
									ETag = obj.ETag,
									Key = obj.Key,
									LastModified = obj.LastModified,
									Size = obj.Size,
								};
							reusedProgressArgs.Objects.Clear();
							reusedProgressArgs.Objects.AddRange(queryResults);
							ListingProgressed(reusedProgressArgs, () =>
							{
							});
						}
					}
				} while (listRequest != null);

				// Report completion.
				if (ListingCompleted != null)
					ListingCompleted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Listing canceled.");

				// Report cancelation.
				if (ListingCanceled != null)
					ListingCanceled(reusedProgressArgs, exception, () =>
					{
						reusedProgressArgs.State = TransferState.CANCELED;
					});
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Warn("Check the provided AWS Credentials.");
					}
					else
					{
						logger.Warn("Error occurred. Message:'{0}' when listing objects", amznException.Message);
					}
				}
				else
				{
					logger.Warn("Exception occurred: {0}", exception.Message);
				}

				// Report failure.
				if (ListingFailed != null)
					ListingFailed(reusedProgressArgs, exception, () =>
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
