/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class BackupAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public BackupAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterUploadEventHandlers();
			RegisterDeleteEventHandlers();
		}

		public override void DoImplementation(IVersionedFile file, object userData)
		{
			TransferAgent.UploadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
