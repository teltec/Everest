/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Teltec.Storage.Backend
{
	public class TransferAgentOptions
	{
		private long _UploadChunkSizeInBytes;
		public long UploadChunkSizeInBytes
		{
			get { return _UploadChunkSizeInBytes; }
			set { _UploadChunkSizeInBytes = value; }
		}
	}
}
