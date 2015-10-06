using NLog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public class ClientCommandEventArgs : BoundCommandEventArgs
	{
	}

	public class ClientHandler : BaseHandler, IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private ISynchronizeInvoke Owner;
		private string ClientName;
		private string Host;
		private int Port;
		private volatile bool ShouldStopConnectionMonitor;
		private Thread ConnectionMonitor;

		public Client Client { get; internal set; }

		public ClientHandler(ISynchronizeInvoke owner, string clientName, string host, int port)
		{
			Owner = owner;
			ClientName = clientName;
			Host = host;
			Port = port;

			Client = new Client(owner);
			Client.Connected += Client_Connected;
			Client.Disconnected += Client_Disconnected;
			Client.MessageReceived += Client_MessageReceived;

			CreateConnectionMonitorThread();
		}

		~ClientHandler()
		{
			Dispose(false);
		}

		#region Worker thread

		private void DestroyConnectionMonitorThread()
		{
			if (ConnectionMonitor == null)
				return;

			ShouldStopConnectionMonitor = true;
			ConnectionMonitor.Join();

			ConnectionMonitor = null;
		}

		private void CreateConnectionMonitorThread()
		{
			ConnectionMonitor = new Thread(TryToKeepConnectedStatic);
			ConnectionMonitor.IsBackground = true;
			ConnectionMonitor.Start(new ThreadArgument
			{
				Handler = this,
			});
		}

		private class ThreadArgument
		{
			public ClientHandler Handler;

			public delegate void ActionDelegate(ClientHandler handler);
			public ActionDelegate StartCallback;
			public ActionDelegate FinishCallback;
		}

		private static void TryToKeepConnectedStatic(object param)
		{
			ThreadArgument obj = (ThreadArgument)param;
			obj.Handler.TryToKeepConnected(obj);
		}

		private void TryToKeepConnected(ThreadArgument param)
		{
			ClientHandler handler = param.Handler;

			Thread.CurrentThread.Name = "connection_monitor_proc";

			// Start up.
			if (param.StartCallback != null)
				param.StartCallback(handler);

			try
			{
				while (!handler.ShouldStopConnectionMonitor)
				{
					// If it is not connected, then try to connect.
					if (!handler.Client.IsConnected)
					{
						handler.Client.Connect(Host, Port, true);
					}

					// Wait for a disconnection or a connection failure before checking again.
					handler.Client.NeedsReconnectionEvent.WaitOne();

					// If it didn't really connect, then wait and try again.
					if (!handler.Client.IsConnected)
					{
						Thread.Sleep(10000); // Wait 10 seconds before trying to reconnect.
					}
				}
			}
			catch (ThreadInterruptedException)
			{
				// Handle interruption.
				logger.Debug("Interrupted worker {0}", Thread.CurrentThread.Name);
			}
			finally
			{
				logger.Debug("Cleaning up thread {0}", Thread.CurrentThread.Name);

				// Clean up.
				if (param.FinishCallback != null)
					param.FinishCallback(handler);
			}
		}

		#endregion

		public void Send(string cmd)
		{
			byte[] data = StringToBytes(cmd);
			Client.Send(data);
		}

		private void Client_Connected(object sender, ClientConnectedEventArgs e)
		{
			Send(Commands.Register(ClientName));
		}

		private void Client_Disconnected(object sender, ClientConnectedEventArgs e)
		{
			// ...
		}

		private void Client_MessageReceived(object sender, ClientReceiveEventArgs e)
		{
			string message = BytesToString(e.Data);
			if (string.IsNullOrEmpty(message))
			{
				// TODO(jweyrich): Handle invalid message.
				return;
			}

			string[] lines = message.Split('\n');
			foreach (string line in lines)
			{
				if (!string.IsNullOrEmpty(line))
					HandleMessage(line);
			}
		}

		private bool HandleMessage(string message)
		{
			string errorMessage = null;
			Message msg = new Message(message);

			BoundCommand command = Commands.ClientParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				Send(Commands.ReportError(errorMessage));
				return false;
			}

			command.InvokeHandler(this, new ClientCommandEventArgs { Command = command });

			return true;
		}

		private void HandleControl(Message msg)
		{
			string[] validTokens;

			validTokens = new string[]{ "PLAN" };

			string sub1 = msg.NextToken();
			if (string.IsNullOrEmpty(sub1) || !validTokens.Contains(sub1))
			{
				// TODO(jweyrich): Handle invalid command.
				return;
			}

			if (sub1.Equals("PLAN", StringComparison.InvariantCulture))
			{
				validTokens = new string[] { "RUN", "RESUME", "CANCEL", "KILL" };

				string sub2 = msg.NextToken();
				if (string.IsNullOrEmpty(sub1) || !validTokens.Contains(sub2))
				{
					// TODO(jweyrich): Handle invalid command.
					return;
				}

				validTokens = new string[] { "BACKUP", "RESTORE" };
				string sub3 = msg.NextToken();

				if (string.IsNullOrEmpty(sub1) || !validTokens.Contains(sub2))
				{
					// TODO(jweyrich): Handle invalid command.
					return;
				}

				if (sub2.Equals("RUN", StringComparison.InvariantCulture))
				{

				}
				else if (sub2.Equals("RESUME", StringComparison.InvariantCulture))
				{
				}
				else if (sub2.Equals("CANCEL", StringComparison.InvariantCulture))
				{
				}
				else if (sub2.Equals("KILL", StringComparison.InvariantCulture))
				{
				}

			}
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					DestroyConnectionMonitorThread();

					if (Client != null)
					{
						Client.Dispose();
						Client = null;
					}
					this._isDisposed = true;
				}
			}
		}

		/// <summary>
		/// Disposes of all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
