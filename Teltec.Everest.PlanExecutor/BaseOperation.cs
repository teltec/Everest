/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Common;
using Teltec.Storage.Backend;
using Teltec.Storage.Monitor;

namespace Teltec.Everest.PlanExecutor
{
	public interface IBaseOperation : IDisposable
	{
		bool IsRunning { get; }

		IBaseOperationReport GetReport();
		void Start();
		void SendReport();
		void DoEvents();
		void Cancel();
	}

	public abstract class BaseOperation<TReport> : ObservableObject, IBaseOperation where TReport : BaseOperationReport, new()
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public ITransferMonitor TransferListControl; // May be IDisposable, but it's an external reference.
		protected ITransferAgent TransferAgent; // IDisposable
		public TReport Report { get; protected set; }

		#region Properties

		// NOTE: The setter of `IsRunning` is no longer protected because we currently need
		// to change it in `BackupPlanViewControl`.
		public virtual bool IsRunning { get; /* protected */set; }

		public virtual Int32? OperationId { get { throw new NotImplementedException(); } }

		#endregion

		protected BaseOperation()
		{
			Report = new TReport();
			CancellationTokenSource = new CancellationTokenSource();
		}

		public IBaseOperationReport GetReport()
		{
			return Report;
		}

		public abstract void Start();
		public abstract void SendReport();

		public virtual void DoEvents()
		{
			if (TransferAgent == null)
				return;

			TransferAgent.EventDispatcher.DoEvents();
		}

		#region Task

		protected CancellationTokenSource CancellationTokenSource; // IDisposable

		protected Task<T> ExecuteOnBackround<T>(Func<T> action, CancellationToken token)
		{
			return Task.Factory.StartNew<T>(action, token);
		}

		public virtual void Cancel()
		{
			Assert.IsTrue(IsRunning);
		}

		#endregion

		#region Logging

		public System.Diagnostics.EventLog EventLog;

		protected void Log(System.Diagnostics.EventLogEntryType type, string message)
		{
			if (EventLog != null)
				EventLog.WriteEntry(message, type);

			switch (type)
			{
				case System.Diagnostics.EventLogEntryType.Error:
					logger.Error(message);
					break;
				case System.Diagnostics.EventLogEntryType.Warning:
					logger.Warn(message);
					break;
				case System.Diagnostics.EventLogEntryType.Information:
					logger.Info(message);
					break;
			}
		}

		protected void Log(System.Diagnostics.EventLogEntryType type, string format, params object[] args)
		{
			string message = string.Format(format, args);
			Log(type, message);
		}

		protected void Warn(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, message);
		}

		protected void Warn(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, format, args);
		}

		protected void Error(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, message);
		}

		protected void Error(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, format, args);
		}

		protected void Info(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, message);
		}

		protected void Info(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, format, args);
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
					if (TransferAgent != null)
					{
						TransferAgent.Dispose();
						TransferAgent = null;
					}

					if (CancellationTokenSource != null)
					{
						CancellationTokenSource.Dispose();
						CancellationTokenSource = null;
					}
				}
				this._isDisposed = true;
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
