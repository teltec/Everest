using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public class ServerEventArgs : PeerEventArgs
	{
		public Server.ClientContext Context;
	}

	public class ServerConnectedEventArgs : ServerEventArgs
	{
	}

	public class ServerReceiveEventArgs : ServerEventArgs
	{
		public int BytesReceived;
		public byte[] Data;
	}

	public class ServerSendEventArgs : ServerEventArgs
	{
		public int BytesSent;
	}

	public delegate void ServerConnectEventHandler(object sender, ServerConnectedEventArgs e);
	public delegate void ServerReceiveEventHandler(object sender, ServerReceiveEventArgs e);
	public delegate void ServerSendEventHandler(object sender, ServerSendEventArgs e);

	public class Server : Shared, IDisposable
	{
		public class LocalContext
		{
			public Socket ServerSocket;
			public Thread AcceptorThread;

			public void Reset()
			{
				if (ServerSocket != null)
				{
					ServerSocket = null;
				}
				if (AcceptorThread != null)
				{
					AcceptorThread = null;
				}
			}
		}

		public class ClientContext : IDisposable
		{
			public static readonly int BufferSize = 4096; // 4 KB
			public byte[] RecvBuffer = new byte[BufferSize];
			//public ConcurrentQueue<byte[]> InBuffer = new ConcurrentQueue<byte[]>();
			public ConcurrentQueue<byte[]> OutBuffer = new ConcurrentQueue<byte[]>();
			public volatile bool ShouldStop = false;

			public object Tag { get; set; }

			private Socket _ClientSocket;
			public Socket ClientSocket
			{
				get { return _ClientSocket; }
				set
				{
					_ClientSocket = value;
					if (_ClientSocket != null)
					{
						IPEndPoint endpoint = (IPEndPoint)ClientSocket.RemoteEndPoint;
						ClientKey = endpoint.ToString();
					}
				}
			}
			public Thread WorkerThread = null;

			public string ClientKey { get; internal set; }

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
						if (ClientSocket != null)
						{
							ClientSocket.Dispose();
							ClientSocket = null;
						}

						// NOTE: Do not wait or terminate `WorkerThread` because it was not created here.

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

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private LocalContext Context = new LocalContext();

		private volatile bool ShouldStop = false;
		private AutoResetEvent ServerFinished = new AutoResetEvent(false);
		private Dictionary<string, ClientContext> Clients = new Dictionary<string, ClientContext>();
		private List<Thread> ClientThreads = new List<Thread>();

		private void Reset()
		{
			Context.Reset();
			ShouldStop = false;
			Clients.Clear();
			ClientThreads.Clear();
			LocalIP = null;
			LocalPort = short.MaxValue;
		}

		public Server(ISynchronizeInvoke owner) : base(owner)
		{
		}

		~Server()
		{
            Dispose(false);
        }

		#region Events

		private AutoResetEvent InternalConnectedEvent = new AutoResetEvent(false);
		public AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
		public event ServerConnectEventHandler Connected;

		public AutoResetEvent DisconnectedEvent = new AutoResetEvent(false);
		public event ServerConnectEventHandler Disconnected;

		public AutoResetEvent MessageReceivedEvent = new AutoResetEvent(false);
		public event ServerReceiveEventHandler MessageReceived;

		public AutoResetEvent MessageSentEvent = new AutoResetEvent(false);
		public event ServerSendEventHandler MessageSent;

		#endregion

		#region Properties

		private bool _IsRunning = false;
		public bool IsRunning
		{
			get { return _IsRunning; }
			private set { _IsRunning = value;  }
		}

		private string _LocalIP;
		public string LocalIP
		{
			get { return _LocalIP; }
			internal set { _LocalIP = value; }
		}

		private int _LocalPort = short.MaxValue;
		public int LocalPort
		{
			get { return _LocalPort; }
			internal set
			{
				if (value < 1 || value > short.MaxValue)
					throw new ArgumentException("Invalid port number");
				_LocalPort = value;
			}
		}

		#endregion

		#region Socket options

		public bool KeepAlive
		{
			get { return GetKeepAlive(Context.ServerSocket); }
		}

		#endregion

		public void Start(string ipAddressToBind, int port, int backlog = 5, bool enableKeepAlive = false)
		{
			Reset();

			IsRunning = true;

			LocalIP = ipAddressToBind;
			LocalPort = port;

			Context.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			if (enableKeepAlive)
				SetKeepAlive(Context.ServerSocket, enableKeepAlive);

			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(LocalIP), LocalPort);

			Context.ServerSocket.Bind(endpoint);
			Context.ServerSocket.Listen(backlog);

			Context.AcceptorThread = new Thread(() =>
				{
					Thread.CurrentThread.Name = "acceptor_proc";
					logger.Debug("LISTENING ON {0}:{1}", LocalIP, LocalPort);

					try
					{
						while (!ShouldStop)
						{
							Context.ServerSocket.BeginAccept(new AsyncCallback(EndAccept), null);

							// Wait for a connection before accepting another.
							while (!ShouldStop && !InternalConnectedEvent.WaitOne(500))
							{
								//logger.Debug("Waiting connection...");
							}
						}
					}
					catch (ThreadInterruptedException)
					{
						// Handle interruption.
						logger.Debug("Interrupted acceptor.");
					}
					finally
					{
						// Clean up.
						logger.Debug("Cleaning up acceptor.");
					}
				});

			Context.AcceptorThread.Start();
		}

		public void RequestStop()
		{
			if (ShouldStop)
				return;

			// Stop acceptor before stopping the clients.
			ShouldStop = true;

			// Stop he acceptor.
			if (Context.ServerSocket != null)
			{
				Context.ServerSocket.Close();
				//Context.ServerSocket = null;
			}

			lock (Clients)
			{
				// Stop the clients.
				foreach (var entry in Clients)
				{
					entry.Value.ShouldStop = true;
				}
			}
		}

		public void Wait()
		{
			if (!ShouldStop)
				return;

			Thread[] threads = null;

			lock (ClientThreads)
				// Convert to array to reduce lock contetion.
				threads = ClientThreads.ToArray();

			// Wait for all clients to finish.
			foreach (var thread in threads)
			{
				if (thread != null && thread.IsAlive)
					thread.Join();
			}

			// Wait for the acceptor to finish.
			if (Context.AcceptorThread != null && Context.AcceptorThread.IsAlive)
				Context.AcceptorThread.Join();

			IsRunning = false;
		}

		#region Worker thread

		private class ThreadArgument
		{
			public Server Server;
			public ClientContext ClientContext;

			public delegate void ActionDelegate(ClientContext context);
			public ActionDelegate StartCallback;
			public ActionDelegate FinishCallback;
		}

		private static void ProcessClientStatic(object param)
		{
			ThreadArgument obj = (ThreadArgument)param;
			obj.Server.ProcessClient(obj);
		}

		private void TryBeginReceive(ClientContext context)
		{
			// Read data and store in RecvBuffer.
			try
			{
				byte[] buffer = context.RecvBuffer;
				context.ClientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
					new AsyncCallback(EndReceive), context);
			}
			catch (SocketException ex)
			{
				logger.Error("ERROR BeginReceive: {0}", ex.Message);
				HandleSocketError(ex.SocketErrorCode, context);
			}
		}

		private void ProcessClient(ThreadArgument param)
		{
			ClientContext context = param.ClientContext;

			Thread.CurrentThread.Name = string.Format("client_proc-{0}", context.ClientKey);

			// Start up.
			if (param.StartCallback != null)
				param.StartCallback(context);

			try
			{
				int pollTimeout = 1000; // In microseconds

				while (!context.ShouldStop)
				{
					try
					{
						// Check whether the previous read indicated a client disconnection.
						if (context.ClientSocket == null)
							break;

						bool canWrite = context.ClientSocket.Poll(pollTimeout, SelectMode.SelectWrite);
						// true if:
						// - A Connect method call has been processed, and the connection has succeeded.
						// - Data can be sent.
						if (canWrite)
						{
							// Dequeue data from write queue and send it.
							if (!context.OutBuffer.IsEmpty)
							{
								byte[] buffer = null;
								context.OutBuffer.TryDequeue(out buffer);

								try
								{
									context.ClientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None,
										new AsyncCallback(EndSend), context);
								}
								catch (SocketException ex)
								{
									logger.Error("ERROR BeginSend: {0}", ex.Message);
									HandleSocketError(ex.SocketErrorCode, context);
								}
							}
						}
					}
					catch (ObjectDisposedException)
					{
						// Socket was already closed.
					}

					Thread.Sleep(1);
				}
			}
			catch (ThreadInterruptedException)
			{
				// Handle interruption.
				logger.Debug("Interrupted worker for client {0}.", context.ClientKey);
			}
			finally
			{
				logger.Debug("Cleaning up worker for client {0}.", context.ClientKey);

				// Clean up.
				if (param.FinishCallback != null)
					param.FinishCallback(context);
			}
		}

#endregion

		//public int Receive(ClientContext context, out byte[] data)
		//{
		//	if (!context.InBuffer.IsEmpty)
		//	{
		//		bool removed = context.InBuffer.TryDequeue(out data);
		//		return removed ? data.Length : 0;
		//	}
		//	else
		//	{
		//		data = null;
		//		return 0;
		//	}
		//}

		public void Send(ClientContext context, byte[] data)
		{
			if (context == null || context.ClientSocket == null)
				return;

			int count = data.Length;
			byte[] copiedData = new byte[count];
			Buffer.BlockCopy(data, 0, copiedData, 0, count);

			context.OutBuffer.Enqueue(copiedData);
		}

		private void DisconnectClient(ClientContext context)
		{
			if (context.ClientSocket == null)
				return;

			try
			{
				context.ClientSocket.BeginDisconnect(true, new AsyncCallback(EndDisconnect), context);
			}
			catch (ObjectDisposedException)
			{
				// Socket was already closed.
			}
			catch (SocketException ex)
			{
				logger.Error("ERROR BeginDisconnect: {0}", ex.Message);
				HandleSocketError(ex.SocketErrorCode, context);
			}
		}

		#region Socket callbacks

		private void EndAccept(IAsyncResult iar)
		{
			if (iar == null)
				return;

			try
			{
				Socket clientSocket = Context.ServerSocket.EndAccept(iar);
				clientSocket.NoDelay = true;

				ClientContext clientContext = new ClientContext
				{
					ClientSocket = clientSocket,
				};

				clientContext.WorkerThread = new Thread(ProcessClientStatic);
				clientContext.WorkerThread.IsBackground = true;
				clientContext.WorkerThread.Start(new ThreadArgument
				{
					Server = this,
					ClientContext = clientContext,
					StartCallback = (ctx) =>
						{
							lock (Clients)
								Clients.Add(clientContext.ClientKey, clientContext);

							lock (ClientThreads)
								ClientThreads.Add(clientContext.WorkerThread);
						},
					FinishCallback = (ctx) =>
						{
							DisconnectClient(ctx);

							lock (ClientThreads)
								ClientThreads.Remove(ctx.WorkerThread);

							ctx.WorkerThread = null;

							lock (Clients)
								Clients.Remove(ctx.ClientKey);
						},
				});

				OnConnected(new ServerConnectedEventArgs { Context = clientContext });

				// Notify a connection has been made so the acceptor can now wait for another.
				InternalConnectedEvent.Set();
				ConnectedEvent.Set();

				TryBeginReceive(clientContext); // Start receiving.
			}
			catch (ObjectDisposedException ex)
			{
				// Socket was already closed.

				//
				// REFERENCE: https://msdn.microsoft.com/en-us/library/5bb431f9(v=vs.110).aspx
				//
				// To cancel a pending call to the BeginAccept method, close the Socket.
				// When theClosemethod is called while an asynchronous operation is in progress,
				// the callback provided to the BeginAccept method is called. A subsequent call
				// to the EndAcceptmethod will throw an ObjectDisposedException to indicate that
				// the operation has been cancelled.
				//
			}
			catch (Exception ex)
			{
				logger.Error("Caught exception: {0}", ex.Message);
			}
		}

		private void EndDisconnect(IAsyncResult iar)
		{
			if (iar == null || iar.AsyncState == null)
				return;

			ClientContext context = (ClientContext)iar.AsyncState;
			try
			{
				if (context.ClientSocket != null)
					context.ClientSocket.EndDisconnect(iar);

				AfterEndDisconnect(context);
			}
			catch (ObjectDisposedException)
			{
				// Socket was already closed.
			}
			catch (SocketException ex)
			{
				logger.Error("ERROR DISCONNECTING: {0}", ex.Message);
				HandleSocketError(ex.SocketErrorCode, context);
			}
		}

		protected override void AfterEndDisconnect(object param)
		{
			ClientContext context = (ClientContext)param;

			// Signal client thread to terminate.
			context.ShouldStop = true;

			CloseSocket(context);

			// TODO(jweyrich): Remove thread from ClientThreads?

			lock (Clients)
				Clients.Remove(context.ClientKey);

			OnDisconnected(new ServerConnectedEventArgs { Context = context });

			DisconnectedEvent.Set();

			//logger.Debug("DISCONNECTED");

			context.Dispose();
		}

		private void EndSend(IAsyncResult iar)
		{
			if (iar == null || iar.AsyncState == null)
				return;

			ClientContext context = (ClientContext)iar.AsyncState;
			try
			{
				int count = context.ClientSocket.EndSend(iar);

				Statistics.TotalBytesSent += count;

				OnMessageSent(new ServerSendEventArgs { Context = context, BytesSent = count });

				MessageSentEvent.Set();
			}
			catch (ObjectDisposedException)
			{
				// Socket was already closed.
			}
			catch (SocketException ex)
			{
				logger.Error("ERROR SENDING: {0}", ex.Message);
				HandleSocketError(ex.SocketErrorCode, context);
			}
		}

		private void EndReceive(IAsyncResult iar)
		{
			if (iar == null || iar.AsyncState == null)
				return;

			ClientContext context = (ClientContext)iar.AsyncState;
			try
			{
				int count = context.ClientSocket.EndReceive(iar);

				if (count > 0)
				{
					// Copy `count` bytes from `RecvBuffer` to `copiedBuffer`.
					byte[] copiedBuffer = new byte[count];
					Buffer.BlockCopy(context.RecvBuffer, 0, copiedBuffer, 0, count);

					Statistics.TotalBytesReceived += count;

					OnMessageReceived(new ServerReceiveEventArgs
					{
						Context = context,
						BytesReceived = count,
						Data = copiedBuffer
					});

					MessageReceivedEvent.Set();

					TryBeginReceive(context); // Continue receiving.
				}
				else
				{
					AfterEndDisconnect(context);
				}
			}
			catch (ObjectDisposedException)
			{
				// Socket was already closed.
			}
			catch (SocketException ex)
			{
				logger.Error("ERROR RECEIVING: {0}", ex.Message);
				HandleSocketError(ex.SocketErrorCode, context);
			}
		}

		#endregion

		private void CloseSocket(ClientContext context)
		{
			if (context.ClientSocket != null)
			{
				context.ClientSocket.Shutdown(SocketShutdown.Both);
				context.ClientSocket.Close();
				//context.ClientSocket = null;
			}
		}

		#region Delegate invokers

		protected virtual void OnConnected(ServerConnectedEventArgs e)
		{
			logger.Debug("CONNECTION FROM {0}", e.Context.ClientKey);
			InvokeDelegate(Connected, e);
		}

		protected virtual void OnDisconnected(ServerConnectedEventArgs e)
		{
			logger.Debug("DISCONNECTED");
			InvokeDelegate(Disconnected, e);
		}

		protected virtual void OnMessageReceived(ServerReceiveEventArgs e)
		{
			logger.Debug("RECEIVED FROM {0}: {1}",
				e.Context.ClientKey,
				Encoding.ASCII.GetString(e.Data, 0, e.Data.Length));
			InvokeDelegate(MessageReceived, e);
		}

		protected virtual void OnMessageSent(ServerSendEventArgs e)
		{
			InvokeDelegate(MessageSent, e);
		}

		#endregion

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
					RequestStop();
					Wait();
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
