/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class SynchronizationAgent<T> : AbstractSyncAgent<T> where T : IVersionedFile
	{
		public SynchronizationAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterListingEventHandlers();
		}

		public override void DoImplementation(string prefix, bool recursive, object userData)
		{
			TransferAgent.List(prefix, recursive, userData);
		}
	}
}

