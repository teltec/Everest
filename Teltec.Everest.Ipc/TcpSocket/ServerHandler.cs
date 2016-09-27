/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teltec.Common.Extensions;
using Teltec.Everest.Ipc.Protocol;

namespace Teltec.Everest.Ipc.TcpSocket
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
		object ClientsByNameLock = new Object();

		public Server Server { get; internal set; }

		public ServerHandler(ISynchronizeInvoke owner)
		{
			Owner = owner;
			ClientsByName = new Dictionary<string, ClientState>();

			Server = new Server(owner);
			Server.Connected += Server_Connected;
			Server.MessageReceived += Server_MessageReceived;
			Server.Disconnected += Server_Disconnected;

			RegisterCommandHandlers();
		}

		public delegate void ServerCommandHandler(object sender, ServerCommandEventArgs e);

		public event ServerCommandHandler OnControlPlanQuery;
		public event ServerCommandHandler OnControlPlanRun;
		public event ServerCommandHandler OnControlPlanResume;
		public event ServerCommandHandler OnControlPlanCancel;
		public event ServerCommandHandler OnControlPlanKill;

		private void RegisterCommandHandlers()
		{
			Commands.SRV_ERROR.Handler += OnError;
			Commands.SRV_REGISTER.Handler += OnRegister;
			Commands.SRV_ROUTE.Handler += OnRoute;
			Commands.SRV_BROADCAST.Handler += OnBroadcast;
			Commands.SRV_CONTROL_PLAN_QUERY.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanQuery != null)
					OnControlPlanQuery(this, (ServerCommandEventArgs)e);
			};
			Commands.SRV_CONTROL_PLAN_RUN.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanRun != null)
					OnControlPlanRun(this, (ServerCommandEventArgs)e);
			};
			Commands.SRV_CONTROL_PLAN_RESUME.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanResume != null)
					OnControlPlanResume(this, (ServerCommandEventArgs)e);
			};
			Commands.SRV_CONTROL_PLAN_CANCEL.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanCancel != null)
					OnControlPlanCancel(this, (ServerCommandEventArgs)e);
			};
			Commands.SRV_CONTROL_PLAN_KILL.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanKill != null)
					OnControlPlanKill(this, (ServerCommandEventArgs)e);
			};
		}

		public bool IsRunning
		{
			get { return Server.IsRunning; }
		}

		private bool IsRegistered(Server.ClientContext context)
		{
			string clientName = (string)context.Tag;
			ClientState state = null;

			bool found;
			lock (ClientsByNameLock)
				found = ClientsByName.TryGetValue(clientName, out state);

			if (!found)
			{
				logger.Error("The client named {0} is not connected?", clientName);
				return false;
			}

			if (!state.IsRegistered)
			{
				Send(context, Commands.ReportError((int)Commands.ErrorCode.NOT_AUTHORIZED, "Not authorized"));
				return false;
			}

			return true;
		}

		public ClientState GetClientState(string clientName)
		{
			ClientState state;
			lock (ClientsByNameLock)
				ClientsByName.TryGetValue(clientName, out state);
			return state;
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

			lock (ClientsByNameLock)
				ClientsByName.Add(clientName, newState);
		}

		private void Server_Disconnected(object sender, ServerConnectedEventArgs e)
		{
			string clientName = (string)e.Context.Tag;
			if (string.IsNullOrEmpty(clientName))
				return;

			lock (ClientsByNameLock)
			{
				if (ClientsByName.ContainsKey(clientName))
					ClientsByName.Remove(clientName);
			}
		}

		private void Server_MessageReceived(object sender, ServerReceiveEventArgs e)
		{
			string clientName = (string)e.Context.Tag;

			lock (ClientsByNameLock)
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
			Message msg = null;

			try
			{
				msg = new Message(message);
			}
			catch (Exception ex)
			{
				errorMessage = string.Format("Couldn't construct message: {0}", ex.Message);
				logger.Warn(errorMessage);
				Send(context, Commands.ReportError((int)Commands.ErrorCode.INVALID_CMD, errorMessage));
				return false;
			}

			BoundCommand command = Commands.ServerParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				logger.Warn("Did not accept the message: {0}", message);
				Send(context, Commands.ReportError((int)Commands.ErrorCode.INVALID_CMD, errorMessage));
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

		private void OnError(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;
			string message = args.Command.GetArgumentValue<string>("message");

			logger.Warn("ERROR FROM CLIENT: {0}", message);
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

			lock (ClientsByNameLock)
			{
				if (ClientsByName.ContainsKey(newClientName))
				{
					Send(args.Context, Commands.ReportError((int)Commands.ErrorCode.NAME_ALREADY_IN_USE, "This name is already in use"));
					return;
				}

				ClientState state = ClientsByName[oldClientName];
				state.IsRegistered = true;
				args.Context.Tag = newClientName;

				// Remove & re-add state using the new client name.
				ClientsByName.Remove(oldClientName);
				ClientsByName.Add(newClientName, state);

				// Remove this client from the list of unknown target hits if it's there.
				UnknownTargetHits_Remove(newClientName);
			}
		}

		private void OnRoute(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string targetName = args.Command.GetArgumentValue<string>("targetName");
			string message = args.Command.GetArgumentValue<string>("message");

			if (string.IsNullOrEmpty(targetName) || targetName.Length > Commands.REGISTER_CLIENT_NAME_MAXLEN)
			{
				// Don't spam the sender with error messages if there's no GUI app running.
				if (!targetName.Equals(Commands.IPC_DEFAULT_GUI_CLIENT_NAME))
					Send(args.Context, Commands.ReportError((int)Commands.ErrorCode.INVALID_ROUTE_MSG, "Invalid route message"));
				return;
			}

			ClientState targetState = null;

			bool found;
			lock (ClientsByNameLock)
				found = ClientsByName.TryGetValue(targetName, out targetState);
			if (!found)
			{
				bool canProceed = UnknownTargetHits_Hit(targetName);
				if (canProceed)
					Send(args.Context, Commands.ReportError((int)Commands.ErrorCode.UNKNOWN_TARGET, "Unknown target {0}", targetName));
				return;
			}

			//Console.WriteLine("SENDING TO {0} -> {1}", targetName, message);
			Send(targetState.Context, message);
		}

		private void OnBroadcast(object sender, EventArgs e)
		{
			ServerCommandEventArgs args = (ServerCommandEventArgs)e;

			string message = args.Command.GetArgumentValue<string>("message");

			lock (ClientsByNameLock)
			{
				foreach (var state in ClientsByName.Values)
				{
					// Do not send it to the sender.
					if (state.Context.ClientKey.Equals(args.Context.ClientKey))
						continue;

					Send(state.Context, message);
				}
			}
		}

		#region Unknown target hits

		//
		// This mechanism is used to avoid spamming the sender of ROUTE messages with
		// ERROR messages when the intended target is not connected.
		//

		private static readonly int UnknownTargetMaxConsecutiveHits = 3;
		private static readonly int UnknownTargetMaxCapacity = 10;
		private Dictionary<string, int> UnknownTargetHits = new Dictionary<string, int>(UnknownTargetMaxCapacity + 1);
		object UnknownTargetHitsLock = new Object();

		//
		// Summary:
		//     Increment the hit counter for a given client and check whether it exceeded
		//     the maximum consecutive hits.
		//
		// Returns:
		//     true if the caller did not exceed the maximum consecutive hits and may continue
		//     the intended action, false otherwise.
		//
		private bool UnknownTargetHits_Hit(string clientName)
		{
			lock (UnknownTargetHitsLock)
			{
				if (UnknownTargetHits.ContainsKey(clientName))
					UnknownTargetHits[clientName]++;
				else
					UnknownTargetHits[clientName] = 1;

				// Our dict can store at most `UnknownTargetMaxCapacity` items.
				if (UnknownTargetHits.Count > UnknownTargetMaxCapacity) // Count is O(1)
				{
					// Remove the key with fewer hits.
					string keyToBeRemoved = UnknownTargetHits.MinBy(kvp => kvp.Value).Key;
					UnknownTargetHits.Remove(keyToBeRemoved);
				}

				// Did reach maximum consecutive hits for this target?
				if (UnknownTargetHits[clientName] > UnknownTargetMaxConsecutiveHits)
					return false;
			}

			return true;
		}

		private void UnknownTargetHits_Remove(string clientName)
		{
			lock (UnknownTargetHitsLock)
				UnknownTargetHits.Remove(clientName);
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
