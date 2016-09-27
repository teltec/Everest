/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Text;

namespace Teltec.Everest.Ipc.TcpSocket
{
	public abstract class BaseHandler
	{
		protected string BytesToString(byte[] data)
		{
			return Encoding.UTF8.GetString(data);
		}

		protected byte[] StringToBytes(string message)
		{
			return Encoding.UTF8.GetBytes(message + "\n");
		}
	}
}
