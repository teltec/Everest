using System;
using System.Windows.Forms;
using Teltec.Backup.Ipc.Protocol;

namespace Teltec.Backup.App.Forms
{
	internal sealed class RemoteOperation
	{
		public bool RequestedInitialInfo = false;
		public bool GotInitialInfo = false;

		public bool StartedDurationTimer = false;
		private Timer DurationTimer;

		public bool IsRunning { get { return !Status.IsEnded(); } }
		public Commands.OperationStatus Status;
		public DateTime? LastRunAt = null;
		public DateTime? LastSuccessfulRunAt = null;
		public DateTime? StartedAt = null;
		public DateTime? FinishedAt = null;
		//public bool OperationNeedsResume { get { return Status == Commands.OperationStatus.INTERRUPTED; } }

		public RemoteOperation(System.ComponentModel.IContainer components, System.EventHandler timerTick)
		{
			DurationTimer = new System.Windows.Forms.Timer(components);
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
	}
}
