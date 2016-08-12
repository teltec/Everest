using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Teltec.FileSystem;
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

		TransferAgentOptions Options;
		AmazonS3Client _s3Client; // IDisposable
		AmazonS3Config _s3Config;
		string _awsBuckeName;
		bool _shouldDispose = false;
		bool _isDisposed;

		#region Constructors

		public S3StorageBackend(TransferAgentOptions options, AWSCredentials awsCredentials, AmazonS3Config s3Config, string awsBucketName)
		{
			this._shouldDispose = true;

			Options = options;
			SanitizeOptions();

			this._s3Config = s3Config;
			this._s3Config.RegionEndpoint = GetBucketEndpoint(awsCredentials, awsBucketName);
			this._s3Client = new AmazonS3Client(awsCredentials, s3Config);
			this._awsBuckeName = awsBucketName;
		}

		#endregion

		private Amazon.RegionEndpoint GetBucketEndpoint(AWSCredentials awsCredentials, string awsBucketName)
		{
			Amazon.RegionEndpoint defaultResult = Amazon.RegionEndpoint.USEast1;
			Amazon.RegionEndpoint result = null;
			AmazonS3Client temporaryS3Client = null;

			try
			{
				temporaryS3Client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.USEast1);
				GetBucketLocationResponse bucketRegionResponse = temporaryS3Client.GetBucketLocation(awsBucketName);
				result = Amazon.RegionEndpoint.GetBySystemName(bucketRegionResponse.Location.Value);
				if (result == null || string.IsNullOrEmpty(result.SystemName))
					result = defaultResult;
				logger.Debug("The '{0}' bucket is located in the '{1}' region.", awsBucketName, result.DisplayName);
			}
			catch (Exception)
			{
				logger.Log(LogLevel.Warn, "Failed to discover the bucket location for '{0}'. Assuming '{1}'.", awsBucketName, defaultResult.DisplayName);
				result = defaultResult;
			}
			finally
			{
				if (temporaryS3Client != null)
					temporaryS3Client.Dispose();
			}

			return result;
		}

		private void SanitizeOptions()
		{
			if (Options.UploadChunkSizeInBytes < AbsoluteMinPartSize)
				Options.UploadChunkSizeInBytes = AbsoluteMinPartSize;
		}

		#region Upload

		//
		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/API/mpUploadUploadPart.html
		// > Part numbers can be any number from 1 to 10,000, inclusive.
		// > A part number uniquely identifies a part and also defines its
		// > position within the object being created. If you upload a new
		// > part using the same part number that was used with a previous
		// > part, the previously uploaded part is overwritten. Each part
		// > must be at least 5 MB in size, except the last part. There is
		// > no size limit on the last part of your multipart upload.
		//
		public static readonly long AbsoluteMaxNumberOfParts = 10000;
		public static readonly long AbsoluteMinPartSize = 5 * 1024 * 1024; // 5 MiB

		public static long CalculatePartSize(long fileSize, long minPartSize, long maxNumberOfParts)
		{
			double partSize = Math.Ceiling((double)fileSize / maxNumberOfParts);
			if (partSize < minPartSize)
			{
				partSize = minPartSize;
			}
			return (long)partSize;
		}

		public long CalculatePartSize(long fileSize)
		{
			// If fileSize > 48.828125 GiB (50 GB) (`AbsoluteMinPartSize * AbsoluteMaxNumberOfParts`),
			// then partSize will be > 5 MiB (`AbsoluteMinPartSize`).
			return CalculatePartSize(fileSize, Options.UploadChunkSizeInBytes, AbsoluteMaxNumberOfParts);
		}

		public override void UploadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken)
		{
			if (cancellationToken != null)
				cancellationToken.ThrowIfCancellationRequested();

			TransferFileProgressArgs reusedProgressArgs = new TransferFileProgressArgs
			{
				UserData = userData,
				State = TransferState.PENDING,
				TotalBytes = 0,
				TransferredBytes = 0,
				FilePath = filePath,
			};

			TransferUtility fileTransferUtility = null;  // IDisposable

			// REFERENCES:
			//   https://docs.aws.amazon.com/AmazonS3/latest/dev/HLTrackProgressMPUDotNet.html
			//   https://docs.aws.amazon.com/AmazonS3/latest/dev/LLuploadFileDotNet.html
			try
			{
				long fileLength = ZetaLongPaths.ZlpIOHelper.GetFileLength(filePath);

				reusedProgressArgs.TotalBytes = fileLength;

				TransferUtilityConfig xferConfig = new TransferUtilityConfig
				{
					ConcurrentServiceRequests = 10, // Maximum allowed concurrent requests for this file alone.
					MinSizeBeforePartUpload = AbsoluteMinPartSize,
				};

				fileTransferUtility = new TransferUtility(this._s3Client, xferConfig);

				// Step 1: Initialize.
				// Use TransferUtilityUploadRequest to configure options.
				TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
				{
					BucketName = this._awsBuckeName,
					Key = keyName,
					FilePath = filePath,
					CannedACL = S3CannedACL.Private,
					StorageClass = S3StorageClass.ReducedRedundancy,
					PartSize = CalculatePartSize(fileLength),
				};

				uploadRequest.UploadProgressEvent += new EventHandler<UploadProgressArgs>(
					(object sender, UploadProgressArgs e) =>
					{
						// Process event.
						//logger.Debug("PROGRESS {0} -> {1}", filePath, e.ToString());

						long delta = e.TransferredBytes - reusedProgressArgs.TransferredBytes;

						// Report progress.
						if (UploadProgressed != null)
							UploadProgressed(reusedProgressArgs, () =>
							{
								reusedProgressArgs.State = TransferState.TRANSFERRING;
								reusedProgressArgs.DeltaTransferredBytes = delta;
								reusedProgressArgs.TransferredBytes = e.TransferredBytes;
							});
					});

				// Report start - before any possible failures.
				if (UploadStarted != null)
					UploadStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				// TODO(jweyrich): Make it interruptible - use UploadAsync?
				fileTransferUtility.Upload(uploadRequest);

				// Report completion.
				if (UploadCompleted != null)
					UploadCompleted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Upload canceled: {0}", filePath);

				// Report cancelation.
				if (UploadCanceled != null)
				{
					UploadCanceled(reusedProgressArgs, () =>
					{
						reusedProgressArgs.Exception = exception;
						reusedProgressArgs.State = TransferState.CANCELED;
					});
				}
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Log(LogLevel.Warn, "Check the provided AWS Credentials.");
					}
					else
					{
						logger.Log(LogLevel.Warn, "Error occurred during the upload of {0}\nMessage:'{1}'", filePath, amznException.Message);
					}
				}
				else
				{
					logger.Log(LogLevel.Warn, "Exception occurred during the upload of {0}\nException: {1}", filePath, exception.Message);
				}

				// Report failure.
				if (UploadFailed != null)
				{
					UploadFailed(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.FAILED;
						reusedProgressArgs.Exception = exception;
					});
				}
			}
			finally
			{
				if (fileTransferUtility != null)
					fileTransferUtility.Dispose();
			}
		}

		#endregion

		#region Download

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html
		public override void DownloadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken)
		{
			if (cancellationToken != null)
				cancellationToken.ThrowIfCancellationRequested();

			TransferFileProgressArgs reusedProgressArgs = new TransferFileProgressArgs
			{
				UserData = userData,
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
				string fileDirectoryName = FileManager.UnsafeGetDirectoryName(filePath);
				FileManager.UnsafeCreateDirectory(fileDirectoryName);

				// Report start - before any possible failures.
				if (DownloadStarted != null)
					DownloadStarted(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.STARTED;
					});

				const int DefaultBufferSize = 8192; // S3Constants.DefaultBufferSize

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
							reusedProgressArgs.State = TransferState.TRANSFERRING;
							reusedProgressArgs.TotalBytes = downloadResponse.ContentLength;
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
								reusedProgressArgs.DeltaTransferredBytes = bytesRead;
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
				logger.Info("Download canceled: {0}", filePath);

				// Report cancelation.
				if (DownloadCanceled != null)
				{
					DownloadCanceled(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.CANCELED;
						reusedProgressArgs.Exception = exception;
					});
				}
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Log(LogLevel.Warn, "Check the provided AWS Credentials.");
					}
					else
					{
						logger.Log(LogLevel.Warn, "Error occurred during the download of {0}\nMessage:'{1}'", filePath, amznException.Message);
					}
				}
				else
				{
					logger.Log(LogLevel.Warn, "Exception occurred during the download of {0}\nException: {1}", filePath, exception.Message);
				}

				// Report failure.
				if (DownloadFailed != null)
				{
					DownloadFailed(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.FAILED;
						reusedProgressArgs.Exception = exception;
					});
				}
			}
		}

		#endregion

		#region Listing

		// REFERENCE: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingObjectKeysUsingNetSDK.html
		public override void List(string prefix, bool recursive, object userData, CancellationToken cancellationToken)
		{
			if (cancellationToken != null)
				cancellationToken.ThrowIfCancellationRequested();

			ListingProgressArgs reusedProgressArgs = new ListingProgressArgs
			{
				UserData = userData,
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

#if DEBUG
						// Process response.
						foreach (S3Object entry in resultObjects)
						{
							Console.WriteLine("Key = {0}, Size = {1}, LastModified = {2}", entry.Key, entry.Size, entry.LastModified);
						}
#endif

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
				{
					ListingCanceled(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.CANCELED;
						reusedProgressArgs.Exception = exception;
					});
				}
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Log(LogLevel.Warn, "Check the provided AWS Credentials.");
					}
					else
					{
						logger.Log(LogLevel.Warn, "Error occurred during listing\nMessage:'{0}'", amznException.Message);
					}
				}
				else
				{
					logger.Log(LogLevel.Warn, "Exception occurred during listing\nException: {0}", exception.Message);
				}

				// Report failure.
				if (ListingFailed != null)
				{
					ListingFailed(reusedProgressArgs, () =>
					{
						reusedProgressArgs.State = TransferState.FAILED;
						reusedProgressArgs.Exception = exception;
					});
				}
			}
		}

		#endregion

		#region Deletion

		public override void DeleteFile(string keyName, object userData, CancellationToken cancellationToken)
		{
			if (cancellationToken != null)
				cancellationToken.ThrowIfCancellationRequested();

			DeletionArgs reusedArgs = new DeletionArgs
			{
				UserData = userData,
				FilePath = keyName,
			};

			DeleteObjectRequest request = new DeleteObjectRequest
			{
				BucketName = this._awsBuckeName,
				Key = keyName
			};

			try
			{
				// Report start - before any possible failures.
				if (DeletionStarted != null)
					DeletionStarted(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.STARTED;
					});

				this._s3Client.DeleteObject(request);

				// Report completion.
				if (DeletionCompleted != null)
					DeletionCompleted(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Deletion canceled.");

				// Report cancelation.
				if (DeletionCanceled != null)
				{
					DeletionCanceled(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.CANCELED;
						reusedArgs.Exception = exception;
					});
				}
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Log(LogLevel.Warn, "Check the provided AWS Credentials.");
					}
					else
					{
						logger.Log(LogLevel.Warn, "Error occurred. Message:'{0}' when deleting object", amznException.Message);
					}
				}
				else
				{
					logger.Log(LogLevel.Warn, "Exception occurred: {0}", exception.Message);
				}

				// Report failure.
				if (DeletionFailed != null)
				{
					DeletionFailed(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.FAILED;
						reusedArgs.Exception = exception;
					});
				}
			}
		}

		public override void DeleteMultipleFiles(List<Tuple<string, object>> keyNamesAndIdentifiers, CancellationToken cancellationToken)
		{
			DeletionArgs reusedArgs = new DeletionArgs
			{
			};

			DeleteObjectsRequest request = new DeleteObjectsRequest
			{
				BucketName = this._awsBuckeName,
				Objects = (from k in keyNamesAndIdentifiers select new KeyVersion() { Key = k.Item1 }).ToList()
				//Quiet = true
			};

			try
			{
				// Report start - before any possible failures.
				if (DeletionStarted != null)
					DeletionStarted(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.STARTED;
					});

				this._s3Client.DeleteObjects(request);

				// Report completion.
				if (DeletionCompleted != null)
					DeletionCompleted(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.COMPLETED;
					});
			}
			catch (OperationCanceledException exception)
			{
				logger.Info("Deletion canceled.");

				// Report cancelation.
				if (DeletionCanceled != null)
				{
					DeletionCanceled(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.CANCELED;
						reusedArgs.Exception = exception;
					});
				}
			}
			catch (DeleteObjectsException exception)
			{
				// Get error details from response.
				DeleteObjectsResponse errorResponse = exception.Response;

				//foreach (DeletedObject deletedObject in errorResponse.DeletedObjects)
				//{
				//	logger.Debug("Deleted object {0}" + deletedObject.Key);
				//}
				foreach (DeleteError deleteError in errorResponse.DeleteErrors)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Error deleting item " + deleteError.Key);
					sb.AppendLine("  Code   : " + deleteError.Code);
					sb.AppendLine("  Message: " + deleteError.Message);

					logger.Log(LogLevel.Warn, sb.ToString());
				}

				// Report failure.
				if (DeletionFailed != null)
				{
					DeletionFailed(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.FAILED;
						reusedArgs.Exception = exception;
					});
				}
			}
			catch (Exception exception)
			{
				if (exception is AmazonS3Exception)
				{
					AmazonS3Exception amznException = exception as AmazonS3Exception;
					if (amznException.ErrorCode != null && (amznException.ErrorCode.Equals("InvalidAccessKeyId") || amznException.ErrorCode.Equals("InvalidSecurity")))
					{
						logger.Log(LogLevel.Warn, "Check the provided AWS Credentials.");
					}
					else
					{
						logger.Log(LogLevel.Warn, "Error occurred. Message:'{0}' when deleting object", amznException.Message);
					}
				}
				else
				{
					logger.Log(LogLevel.Warn, "Exception occurred: {0}", exception.Message);
				}

				// Report failure.
				if (DeletionFailed != null)
				{
					DeletionFailed(reusedArgs, () =>
					{
						reusedArgs.State = TransferState.FAILED;
						reusedArgs.Exception = exception;
					});
				}
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
