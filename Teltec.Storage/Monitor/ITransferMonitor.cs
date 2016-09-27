/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace Teltec.Storage.Monitor
{
	public interface ITransferMonitor
	{
		void ClearTransfers();
		void TransferStarted(object sender, TransferFileProgressArgs args);
		void TransferProgress(object sender, TransferFileProgressArgs args);
		void TransferFailed(object sender, TransferFileProgressArgs args);
		void TransferCanceled(object sender, TransferFileProgressArgs args);
		void TransferCompleted(object sender, TransferFileProgressArgs args);
	}
}
