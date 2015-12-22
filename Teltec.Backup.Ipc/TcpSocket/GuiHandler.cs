using System;
using System.ComponentModel;
using Teltec.Backup.Ipc.Protocol;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public class GuiCommandEventArgs : BoundCommandEventArgs
	{
	}

	public class GuiHandler : ClientHandler
	{
		public GuiHandler(ISynchronizeInvoke owner, string clientName, string host, int port)
			: base(owner, clientName, host, port)
		{
		}

		public delegate void GuiCommandHandler(object sender, GuiCommandEventArgs e);

		public event GuiCommandHandler OnError;
		public event GuiCommandHandler OnReportPlanStatus;
		public event GuiCommandHandler OnReportPlanProgress;

		protected override void RegisterCommandHandlers()
		{
			Commands.GUI_ERROR.Handler += delegate(object sender, EventArgs e)
			{
				if (OnError != null)
					OnError(this, (GuiCommandEventArgs)e);
			};
			Commands.GUI_REPORT_PLAN_STATUS.Handler += delegate(object sender, EventArgs e)
			{
				if (OnReportPlanStatus != null)
					OnReportPlanStatus(this, (GuiCommandEventArgs)e);
			};
			Commands.GUI_REPORT_PLAN_PROGRESS.Handler += delegate(object sender, EventArgs e)
			{
				if (OnReportPlanProgress != null)
					OnReportPlanProgress(this, (GuiCommandEventArgs)e);
			};
		}

		protected override bool HandleMessage(string message)
		{
			string errorMessage = null;
			Message msg = new Message(message);

			BoundCommand command = Commands.GuiParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				Send(Commands.ReportError((int)Commands.ErrorCode.INVALID_CMD, errorMessage));
				return false;
			}

			command.InvokeHandler(this, new GuiCommandEventArgs { Command = command });

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
