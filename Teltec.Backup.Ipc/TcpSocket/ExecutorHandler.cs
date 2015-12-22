using System;
using System.ComponentModel;
using Teltec.Backup.Ipc.Protocol;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public class ExecutorCommandEventArgs : BoundCommandEventArgs
	{
	}

	public class ExecutorHandler : ClientHandler
	{
		public ExecutorHandler(ISynchronizeInvoke owner, string clientName, string host, int port)
			: base(owner, clientName, host, port)
		{
		}

		public delegate void ExecutorCommandHandler(object sender, ExecutorCommandEventArgs e);

		public event ExecutorCommandHandler OnError;
		public event ExecutorCommandHandler OnControlPlanCancel;

		protected override void RegisterCommandHandlers()
		{
			Commands.EXECUTOR_ERROR.Handler += delegate(object sender, EventArgs e)
			{
				ExecutorCommandEventArgs args = (ExecutorCommandEventArgs)e;
				int errorCode = args.Command.GetArgumentValue<int>("errorCode");

				switch (errorCode)
				{
					default:
						break;
					case (int)Commands.ErrorCode.NAME_ALREADY_IN_USE:
						DidSendRegister = false;
						break;
				}

				if (OnError != null)
					OnError(this, args);
			};
			Commands.EXECUTOR_CONTROL_PLAN_CANCEL.Handler += delegate(object sender, EventArgs e)
			{
				if (OnControlPlanCancel != null)
					OnControlPlanCancel(this, (ExecutorCommandEventArgs)e);
			};
		}

		protected override bool HandleMessage(string message)
		{
			string errorMessage = null;
			Message msg = new Message(message);

			BoundCommand command = Commands.ExecutorParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				Send(Commands.ReportError((int)Commands.ErrorCode.INVALID_CMD, errorMessage));
				return false;
			}

			command.InvokeHandler(this, new ExecutorCommandEventArgs { Command = command });

			return true;
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					//if (obj != null)
					//{
					//	obj.Dispose();
					//	obj = null
					//}

					base.Dispose(disposing);
					this._isDisposed = true;
				}
			}
		}

		#endregion
	}
}
