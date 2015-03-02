using System;
using System.Threading;

namespace Teltec.Storage.Backend
{
	public interface IStorageBackend : IDisposable
	{
		#region Upload methods

		void UploadFile(string filePath, string keyName, CancellationToken cancellationToken);

		#endregion
	}
}
