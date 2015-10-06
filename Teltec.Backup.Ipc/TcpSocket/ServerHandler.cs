using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teltec.Backup.Ipc.Protocol;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public class ClientState
	{
		public Server.ClientContext Context { get; internal set; }
		public bool IsRegistered { get; internal set; }
		public DateTime LastSeen { get; internal set; }
	}

	public class ServerCommandEventArgs : BoundCommandEventArgs
	{
		public Server.ClientContext Context;
	}

	public class ServerHandler : BaseHandler, IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private ISynchronizeInvoke Owner;
		private Dictionary<string, ClientState> ClientsByName;

		public Server Server { get; internal set; }

		public ServerHandler(ISynchronizeInvoke owner)
		{
			Commands.SRV_REGISTER.Handler = OnRegister;
			Commands.SRV_ROUTE.Handler = OnRoute;
			Commands.SRV_BROADCAST.Handler = OnBroadcast;
			Commands.SRV_CONTROL_PLAN_RUN.Handler = OnControlPlanRun;
			Commands.SRV_CONTROL_PLAN_RESUME.Handler = OnControlPlanResume;
			Commands.SRV_CONTROL_PLAN_CANCEL.Handler = OnControlPlanCancel;
			Commands.SRV_CONTROL_PLAN_KILL.Handler = OnControlPlanKill;

			Owner = owner;
			Server = new Server(owner);
			Server.Connected += Server_Connected;
			Server.MessageReceived += Server_MessageReceived;
			Server.Disconnected += Server_Disconnected;
			ClientsByName = new Dictionary<string, ClientState>();
		}

		public bool IsRunning
		{
			get { return Server.IsRunning; }
		}

		private bool IsRegistered(Server.ClientContext context)
		{
			string clientName = (string)context.Tag;
			ClientState state = null;
			bool found = ClientsByName.TryGetValue(clientName, out state);
			if (!found)
			{
				logger.Error("The client named {0} is not connected?", clientName);
				return false;
			}

			if (!state.IsRegistered)
			{
				Send(context, Commands.ReportError("Not authorized"));
				return false;
			}

			return true;
		}

		public void Start(string ipAddressToBind, int port)
		{
			Server.Start(ipAddressToBind, port, 5, true);
		}

		public void RequestStop()
		{
			Server.RequestStop();
		}

		public void Wait()
		{
			Server.Wait();
		}

		public void Send(Server.ClientContext context, string cmd)
		{
			byte[] data = StringToBytes(cmd);
			Server.Send(context, data);
		}

		private void Server_Connected(object sender, ServerConnectedEventArgs e)
		{
			string clientName = e.Context.ClientKey;
			e.Context.Tag = clientName;
			ClientState newState = new ClientState
			{
				Context = e.Context,
				IsRegistered = false,
				LastSeen = DateTime.UtcNow,
			};
			ClientsByName.Add(clientName, newState);
		}

		private void Server_Disconnected(object sender, ServerConnectedEventArgs e)
		{
			string clientName = (string)e.Context.Tag;
			if (string.IsNullOrEmpty(clientName))
				return;

			if (ClientsByName.ContainsKey(clientName))
				ClientsByName.Remove(clientName);
		}

		private void Server_MessageReceived(object sender, ServerReceiveEventArgs e)
		{
			string clientName = (string)e.Context.Tag;
			ClientsByName[clientName].LastSeen = DateTime.UtcNow;

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
					HandleMessage(e.Context, line);
			}
		}

		private bool HandleMessage(Server.ClientContext context, string message)
		{
			string errorMessage = null;
			Message msg = new Message(message);

			BoundCommand command = Commands.ServerParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				Send(context, Commands.ReportError(errorMessage));
				return false;
			}

			// Requires authentication?
			if (command.RequiresAuth && !IsRegistered(context))
			{
				//	errorMessage = "Not authorized";
				return false;
			}

			command.InvokeHandler(this, new ServerCommandEventArgs { Context = context, Command = command });

			return true;
		}

		private void OnRegister(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string oldClientName = (string)args.Context.Tag;
			string newClientName = args.Command.GetArgumentValue<string>("clientName");

			if (string.IsNullOrEmpty(newClientName) || newClientName.Length > Commands.REGISTER_CLIENT_NAME_MAXLEN)
			{
				// TODO(jweyrich): Handle invalid registration msg.
				return;
			}

			if (ClientsByName.ContainsKey(newClientName))
			{
				Send(args.Context, Commands.ReportError("This name is already in use"));
				return;
			}

			ClientState state = ClientsByName[oldClientName];
			state.IsRegistered = true;
			args.Context.Tag = newClientName;

			// Remove & re-add state using the new client name.
			ClientsByName.Remove(oldClientName);
			ClientsByName.Add(newClientName, state);
		}

		private void OnControlPlanRun(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string planType = args.Command.GetArgumentValue<string>("planType");
			Int32 planId = args.Command.GetArgumentValue<Int32>("planId");

			// TODO(jweyrich): ...
		}

		private void OnControlPlanResume(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string planType = args.Command.GetArgumentValue<string>("planType");
			Int32 planId = args.Command.GetArgumentValue<Int32>("planId");

			// TODO(jweyrich): ...
		}

		private void OnControlPlanCancel(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string planType = args.Command.GetArgumentValue<string>("planType");
			Int32 planId = args.Command.GetArgumentValue<Int32>("planId");

			// TODO(jweyrich): ...
		}

		private void OnControlPlanKill(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string planType = args.Command.GetArgumentValue<string>("planType");
			Int32 planId = args.Command.GetArgumentValue<Int32>("planId");

			// TODO(jweyrich): ...
		}

		private void OnRoute(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string targetName = args.Command.GetArgumentValue<string>("targetName");
			string message = args.Command.GetArgumentValue<string>("message");

			if (string.IsNullOrEmpty(targetName) || targetName.Length > Commands.REGISTER_CLIENT_NAME_MAXLEN)
			{
				Send(args.Context, Commands.ReportError("Invalid route message"));
				return;
			}

			ClientState targetState = null;
			bool found = ClientsByName.TryGetValue(targetName, out targetState);
			if (!found)
			{
				Send(args.Context, Commands.ReportError(string.Format("Unknown target {0}", targetName)));
				return;
			}

			Console.WriteLine("SENDING TO {0} -> {1}", targetName, message);
			Send(targetState.Context, message);
		}

		private void OnBroadcast(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string message = args.Command.GetArgumentValue<string>("message");

			foreach (var state in ClientsByName.Values)
			{
				// Do not send it to the sender.
				if (state.Context.ClientKey.Equals(args.Context.ClientKey))
					continue;

				Send(state.Context, message);
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
					if (Server != null)
					{
						Server.Dispose();
						Server = null;
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
