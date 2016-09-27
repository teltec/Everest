/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.ComponentModel;
using Teltec.Everest.Ipc.Protocol;

namespace Teltec.Everest.Ipc.TcpSocket
{
	public class GuiCommandEventArgs : BoundCommandEventArgs
	{
	}

	public class GuiHandler : ClientHandler
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
				GuiCommandEventArgs args = (GuiCommandEventArgs)e;
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
			Message msg = null;

			try
			{
				msg = new Message(message);
			}
			catch (Exception ex)
			{
				errorMessage = string.Format("Couldn't construct message: {0}", ex.Message);
				logger.Warn(errorMessage);
				Send(Commands.ReportError((int)Commands.ErrorCode.INVALID_CMD, errorMessage));
				return false;
			}

			BoundCommand command = Commands.GuiParser.ParseMessage(msg, out errorMessage);
			if (command == null)
			{
				logger.Warn("Did not accept the message: {0}", message);
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
