using System;
using System.Collections.Generic;
using System.Threading;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Backend
{
	public interface IStorageBackend : IDisposable
	{
		#region Upload

		void UploadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken);

		#endregion

		#region Download

		void DownloadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken);

		#endregion

		#region Listing

		void List(string prefix, bool recursive, object userData, CancellationToken cancellationToken);

		#endregion

		#region Deletion

		void DeleteFile(string keyName, object userData, CancellationToken cancellationToken);
		void DeleteMultipleFiles(List<Tuple<string, object>> keyNamesAndUserData, CancellationToken cancellationToken);

		#endregion
	}
}
