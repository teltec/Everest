using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Teltec.Backup.Ipc.Net
{
	public class ActiveClients
	{
		// Client list.
		private Dictionary<UInt64, ClientState> _Clients = new Dictionary<UInt64, ClientState>();

		public void Add(ClientState client)
		{
			if (!_Clients.ContainsKey(client.ClientId))
				_Clients.Add(client.ClientId, client);
		}

		public void Remove(ClientState client)
		{
			_Clients.Remove(client.ClientId);
		}

		public ClientState Get(UInt64 clientId)
		{
			return _Clients[clientId];
		}
	}

	// State object for reading client data asynchronously
	public class ClientState
	{
		public UInt64 ClientId;

		// Client  socket.
		public Socket ClientSocket = null;

		// Size of receive buffer.
		public const int BufferSize = 1024;

		// Receive buffer.
		public byte[] ReceiveBuffer = new byte[BufferSize];

		// Send buffer.
		public byte[] SendBuffer = new byte[BufferSize];

		// Received data string.
		public StringBuilder sb = new StringBuilder();
	}

	//
	// Code adapted from https://msdn.microsoft.com/en-us/library/fx6588te%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
	//
	public class AsyncTcpListener
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		// Signal all threads to stop.
		private ManualResetEvent StopEvent = new ManualResetEvent(false);

		// Signal thread a client did finish connecting.
		private ManualResetEvent ClientConnected = new ManualResetEvent(false);

		private IPAddress LocalIP = IPAddress.Loopback;

		private UInt64 CurrentClientId = 0;

		private Socket Listener;

		private ActiveClients ActiveClients = new ActiveClients();

		#region Properties

		private string _LocalAddress = "127.0.0.1";
		public string LocalAddress
		{
			get { return _LocalAddress; }
			private set
			{
				LocalIP = IPAddress.Parse(value);
				_LocalAddress = value;
			}
		}

		private ushort _LocalPort = 50051;
		public ushort LocalPort
		{
			get { return _LocalPort; }
			private set { _LocalPort = value; }
		}

		#endregion

		// Signal all threads to stop.
		public void Stop()
		{
			StopEvent.Set();
		}

		// Process the client connection.
		public void AcceptCallback(IAsyncResult ar)
		{
			// Signal the main thread to continue.
			ClientConnected.Set();

			// Get the socket that handles the client request.
			Socket listener = (Socket)ar.AsyncState;
			Socket socket = listener.EndAccept(ar);

			// Enable Keep Alive.
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

			// Create the state object.
			ClientState state = new ClientState();
			state.ClientSocket = socket;
			ActiveClients.Add(state);

			while (!this.StopEvent.WaitOne(0))
			{
				socket.BeginReceive(state.ReceiveBuffer, 0, ClientState.BufferSize, 0,
					new AsyncCallback(ReadCallback), state);
			}
		}

		public void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;

			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			ClientState state = (ClientState)ar.AsyncState;
			Socket socket = state.ClientSocket;

			// Read data from the client socket.
			int bytesRead = socket.EndReceive(ar);

			if (bytesRead > 0)
			{
				// There  might be more data, so store the data received so far.
				state.sb.Append(Encoding.ASCII.GetString(state.ReceiveBuffer, 0, bytesRead));

				// Check for end-of-file tag. If it is not there, read more data.
				content = state.sb.ToString();
				if (content.IndexOf("<EOF>") > -1)
				{
					// All the data has been read from the client. Display it on the console.
					logger.Debug("Read {0} bytes from socket. \n Data : {1}",
						content.Length, content);

					// Echo the data back to the client.
					Send(socket, content);
				}
				else
				{
					// Not all data received. Get more.
					socket.BeginReceive(state.ReceiveBuffer, 0, ClientState.BufferSize, 0,
						new AsyncCallback(ReadCallback), state);
				}
			}
		}

		private void Send(Socket socket, String data)
		{
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			socket.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), socket);
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket socket = (Socket)ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = socket.EndSend(ar);
				logger.Debug("Sent {0} bytes to client.", bytesSent);

				//socket.Shutdown(SocketShutdown.Both);
				//socket.Close();
			}
			catch (Exception e)
			{
				logger.Error(e.ToString());
			}
		}

		public void StartListening()
		{
			// Local endpoint for the socket.
			IPEndPoint localEndPoint = new IPEndPoint(LocalIP, LocalPort);

			// Create a TCP socket.
			Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and listen for incoming connections.
			try
			{
				Listener.Bind(localEndPoint);
				Listener.Listen(5); // Maximum 5 pending connection requests.

				while (StopEvent.WaitOne(0))
				{
					// Set the event to nonsignaled state.
					ClientConnected.Reset();

					// Start an asynchronous socket to listen for connections.
					logger.Debug("Waiting for a connection...");

					// Accept one client connection asynchronously.
					Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);

					// Wait until a connection is made and processed before continuing.
					while (!ClientConnected.WaitOne(250))
					{
						// Test whether the thread was already signaled to terminate.
						if (StopEvent.WaitOne(0))
							break;
					}
				}
			}
			catch (Exception e)
			{
				logger.Error(e.ToString());
			}
		}
	}
}