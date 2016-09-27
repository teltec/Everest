/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading;

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
