using System;
using System.Windows.Forms;
using Teltec.Everest.Ipc.Protocol;

namespace Teltec.Everest.App.Forms
{
	internal sealed class RemoteOperation : IDisposable
	{
		public bool RequestedInitialInfo = false;
		public bool GotInitialInfo = false;

		public bool StartedDurationTimer = false;
		private readonly Timer DurationTimer; // IDisposable

		public bool IsRunning { get { return !Status.IsEnded(); } }
		public Commands.OperationStatus Status;
		public DateTime? LastRunAt = null;
		public DateTime? LastSuccessfulRunAt = null;
		public DateTime? StartedAt = null;
		public DateTime? FinishedAt = null;
		//public bool OperationNeedsResume { get { return Status == Commands.OperationStatus.INTERRUPTED; } }

		public RemoteOperation(System.EventHandler timerTick)
		{
			DurationTimer = new System.Windows.Forms.Timer();
			DurationTimer.Interval = 1000;
			DurationTimer.Tick += new System.EventHandler(timerTick);
		}

		public void StartTimer()
		{
			StartedDurationTimer = true;
			DurationTimer.Enabled = true;
			DurationTimer.Start();
		}

		public void StopTimer()
		{
			DurationTimer.Stop();
			DurationTimer.Enabled = false;
			StartedDurationTimer = false;
		}

		public void Reset()
		{
			StopTimer();
			RequestedInitialInfo = false;
			GotInitialInfo = false;

			LastRunAt = null;
			LastSuccessfulRunAt = null;
			StartedAt = null;
			FinishedAt = null;
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		private void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					DurationTimer.Dispose();
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
