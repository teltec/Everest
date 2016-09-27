/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class RestoreAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public RestoreAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterDownloadEventHandlers();
		}

		public override void DoImplementation(IVersionedFile file, object userData)
		{
			TransferAgent.DownloadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
