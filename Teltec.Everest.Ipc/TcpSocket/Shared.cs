/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace Teltec.Everest.Ipc.TcpSocket
{
	public class LocalStatistics
	{
		public int TotalBytesSent { get; internal set; }
		public int TotalBytesReceived { get; internal set; }
	}

	public class PeerEventArgs : EventArgs
	{
	}

	public class Shared
	{
		private ISynchronizeInvoke Owner;

		public Shared(ISynchronizeInvoke owner)
		{
			Owner = owner;
		}

		#region Properties

		private LocalStatistics _Statistics = new LocalStatistics();
		public LocalStatistics Statistics
		{
			get { return _Statistics; }
		}

		#endregion

		#region Auxiliar methods

		public static bool GetKeepAlive(Socket socket)
		{
			if (socket == null)
				return false;

			int value = (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive);
			return value == 1 ? true : false;
		}

		public static void SetKeepAlive(Socket socket, bool enabled)
		{
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, enabled ? 1 : 0);
		}

		public static bool IsSocketConnected(Socket socket)
		{
			return socket != null && socket.Connected;
		}

		protected void HandleSocketError(SocketError error, object context)
		{
			switch (error)
			{
				case SocketError.ConnectionReset:
				case SocketError.ConnectionAborted:
					AfterEndDisconnect(context);
					break;
			}
		}

		protected virtual void AfterEndDisconnect(object param)
		{
		}

		protected virtual void InvokeDelegate(Delegate method, EventArgs e)
		{
			if (method != null)
			{
				if (Owner.InvokeRequired)
					Owner.BeginInvoke(method, new object[] { this, e });
				else
					method.DynamicInvoke(this, e);
			}
		}

		#endregion
	}
}
