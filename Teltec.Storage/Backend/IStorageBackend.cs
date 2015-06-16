using System;
using System.Threading;

namespace Teltec.Storage.Backend
{
	public interface IStorageBackend : IDisposable
	{
		#region Upload

		void UploadFile(string filePath, string keyName, CancellationToken cancellationToken);

		#endregion

		#region Download

		void DownloadFile(string filePath, string keyName, CancellationToken cancellationToken);

		#endregion

		#region Listing

		void List(string prefix, bool recursive, CancellationToken cancellationToken);

		#endregion
	}
}
