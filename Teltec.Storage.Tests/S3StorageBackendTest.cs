using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teltec.Storage.Implementations.S3;

namespace Teltec.Storage.Tests
{
	[TestClass]
	public class S3StorageBackendTest
	{
		private long CalculatePartSize(long contentLength)
		{
			return S3StorageBackend.CalculatePartSize(
				contentLength,
				S3StorageBackend.AbsoluteMinPartSize,
				S3StorageBackend.AbsoluteMaxNumberOfParts);
		}

		[TestMethod]
		public void TestCalculatePartSize()
		{
			long contentLength = 0;
			long partSize = 0;

			contentLength = 1; // 1 byte
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);

			contentLength = 1024; // 1 KB
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);

			contentLength = 1024 * 1024 * 1; // 1 MB
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);

			contentLength = 1024 * 1024 * 10; // 10 MB
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);

			contentLength = 1024 * 1024 * 100; // 100 MB
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);

			contentLength = 1024 * 1024 * 1024; // 1 GB
			partSize = CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.AbsoluteMinPartSize, partSize);
		}
	}
}
