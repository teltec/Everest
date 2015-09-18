using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teltec.Storage.Implementations.S3;

namespace Teltec.Storage.Tests
{
	[TestClass]
	public class S3StorageBackendTest
	{
		[TestMethod]
		public void TestCalculatePartSize()
		{
			long contentLength = 0;
			long partSize = 0;

			contentLength = 1; // 1 byte
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);

			contentLength = 1024; // 1 KB
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);

			contentLength = 1024 * 1024 * 1; // 1 MB
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);

			contentLength = 1024 * 1024 * 10; // 10 MB
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);

			contentLength = 1024 * 1024 * 100; // 100 MB
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);

			contentLength = 1024 * 1024 * 1024; // 1 GB
			partSize = S3StorageBackend.CalculatePartSize(contentLength);
			Assert.AreEqual(S3StorageBackend.MinPartSize, partSize);
		}
	}
}
