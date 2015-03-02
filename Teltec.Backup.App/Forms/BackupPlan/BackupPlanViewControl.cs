using App;
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Common;
using Teltec.Common.Extensions;
using Teltec.Storage.Utils;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanViewControl : ObservableUserControl, IDisposable
	{
		private readonly BackupPlanRepository _dao = new BackupPlanRepository();

		public BackupPlanViewControl()
		{
			InitializeComponent();

			this.llblRunNow.Text = RUNNOW_STOPPED;
			this.lblStatus.Text = STATUS_STOPPED;

			this.ModelChangedEvent += (sender, args) =>
			{
				lblTitle.DataBindings.Clear();
				lblSchedule.DataBindings.Clear();

				if (Model == null)
					return;

				Binding lblTitleTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.Name));
				lblTitleTextBinding.Format += lblTitleTextBinding_Format;

				Binding lblScheduleTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.ScheduleType));

				lblTitle.DataBindings.Add(lblTitleTextBinding);
				lblSchedule.DataBindings.Add(lblScheduleTextBinding);
			};
		}

		void lblTitleTextBinding_Format(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType != typeof(string))
				return;

			string value = e.Value as string;

			if (String.IsNullOrEmpty(value))
				e.Value = "(UNNAMED)";
			else
				e.Value = value.ToUpper();
		}

		private void llblDeletePlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Models.BackupPlan plan = Model as Models.BackupPlan;
			_dao.Delete(plan);
			Model = null;
			OnDelete(this, e);
		}

		public delegate void DeleteEventHandler(object sender, EventArgs e);
		public event DeleteEventHandler Deleted;

		protected virtual void OnDelete(object sender, EventArgs e)
		{
			if (Deleted != null)
				Deleted(this, e);
		}

		#region Model

		[
		Bindable(true),
		System.ComponentModel.Category("Data"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
		]
		protected object _Model;
		public virtual object Model
		{
			get { return _Model; }
			set
			{
				SetField(ref _Model, value);
				OnModelChanged(this, new ModelChangedEventArgs(_Model));
			}
		}

		public class ModelChangedEventArgs : EventArgs
		{
			private object _model;
			public object Model
			{
				get { return _model; }
			}

			public ModelChangedEventArgs(object model)
			{
				_model = model;
			}
		}

		public delegate void ModelChangedEventHandler(object sender, ModelChangedEventArgs e);

		public event ModelChangedEventHandler ModelChangedEvent;

		protected virtual void OnModelChanged(object sender, ModelChangedEventArgs e)
		{
			if (ModelChangedEvent != null)
				ModelChangedEvent(this, e);
		}

		#endregion

		#region Collapse

		private bool Collapsed = false;

		private void panelTitle_MouseClick(object sender, MouseEventArgs e)
		{
			Collapsed = !Collapsed;
			if (Collapsed)
			{
				this.Controls.Remove(this.panelContents);
				this.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height - this.panelContents.Size.Height);
			}
			else
			{
				this.Controls.Add(this.panelContents);
				this.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height + this.panelContents.Size.Height);
			}
		}

		#endregion

		private void llblEditPlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var presenter = new NewBackupPlanPresenter(this.Model as Models.BackupPlan))
			{
				presenter.ShowDialog(this.ParentForm);
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateDuration(RunningBackup.IsRunning ? BackupStatus.Updated : BackupStatus.Finished);
		}

		Models.Backup RunningBackup = null;
		BackupResults BackupResults = null;

		static string RUNNOW_RUNNING = "Cancel";
		static string RUNNOW_STOPPED = "Run now";
		static string STATUS_STARTED = "Running";
		static string STATUS_STOPPED = "Stopped";
		static string STATUS_CANCELED = "Canceled";
		static string STATUS_FAILED = "Failed";
		static string STATUS_COMPLETED = "Completed";
		static string DURATION_STARTED = "Starting...";

		private void UpdateStatsInfo(BackupStatus status)
		{
			Assert.IsNotNull(BackupResults);

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case BackupStatus.Started:
					{
						this.lblSources.Text = RunningBackup.Sources;
						this.llblRunNow.Text = RUNNOW_RUNNING;
						this.lblStatus.Text = STATUS_STARTED;
						this.lblDuration.Text = DURATION_STARTED;

						timer1.Enabled = true;
						timer1.Start();
						break;
					}
				case BackupStatus.Finished:
					{
						UpdateDuration(status);
						this.llblRunNow.Text = RUNNOW_STOPPED;
						this.lblStatus.Text = STATUS_COMPLETED;

						timer1.Stop();
						timer1.Enabled = false;
						break;
					}
				case BackupStatus.Updated:
					{
						break;
					}
				case BackupStatus.Failed:
				case BackupStatus.Canceled:
					{
						UpdateDuration(status);
						this.llblRunNow.Text = RUNNOW_STOPPED;
						this.lblStatus.Text = status == BackupStatus.Canceled ? STATUS_CANCELED : STATUS_FAILED;

						timer1.Stop();
						timer1.Enabled = false;
						break;
					}
			}
			
			lblFilesUploaded.Text = string.Format("{0} of {1}",
				BackupResults.Stats.Completed, BackupResults.Stats.Total);
		}

		private void UpdateDuration(BackupStatus status)
		{
			Assert.IsNotNull(RunningBackup);
			var duration = !status.IsEnded()
				? DateTime.UtcNow - RunningBackup.StartedAt
				: RunningBackup.FinishedAt - RunningBackup.StartedAt;
			lblDuration.Text = TimeSpanUtils.GetReadableTimespan(duration);
		}

		private void llblRunNow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (RunningBackup == null || !RunningBackup.IsRunning)
			{
				// IMPORTANT: Dispose before assigning.
				if (RunningBackup != null)
					RunningBackup.Dispose();
				// Create new backup.
				RunningBackup = new Models.Backup(this.Model as Models.BackupPlan);
				RunningBackup.Updated += (sender2, e2) => UpdateStatsInfo(e2.Status);
				//RunningBackup.EventLog = ...
				//RunningBackup.TransferListControl = ...
				RunningBackup.Start(out BackupResults);
			}
			else if (RunningBackup != null && RunningBackup.IsRunning)
			{
				RunningBackup.Cancel();
			}
		}

		//private void eventLog1_EntryWritten(object sender, System.Diagnostics.EntryWrittenEventArgs e)
		//{
		//	string message = string.Format("{0:o} {1} {2}",
		//		e.Entry.TimeWritten, e.Entry.EntryType.ToString().ToUpper(),
		//		e.Entry.Message);
		//	listBox1.Items.Add(message);
		//	// Auto-scroll
		//	listBox1.TopIndex = listBox1.Items.Count - 1;
		//}

		#region Dispose Pattern Implementation

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				if (RunningBackup != null)
				{
					RunningBackup.Dispose();
					RunningBackup = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
