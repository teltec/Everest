using NLog;
using System;
using System.Windows.Forms;
using Teltec.Everest.App.Forms.About;
using Teltec.Everest.App.Forms.BackupPlan;
using Teltec.Everest.App.Forms.NetworkCredentials;
using Teltec.Everest.App.Forms.RestorePlan;
using Teltec.Everest.App.Forms.Settings;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Ipc.TcpSocket;

namespace Teltec.Everest.App.Forms
{
    public partial class MainForm : Form
    {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly BackupPlanRepository _dao = new BackupPlanRepository();

		private void AttachEventHandlers()
		{
			// IMPORTANT: These handlers should be detached on Dispose.
			Provider.Handler.OnError += OnError;
		}

		private void DetachEventHandlers()
		{
			Provider.Handler.OnError -= OnError;
		}

        public MainForm()
        {
			Provider.BuildHandler(this);
			AttachEventHandlers();

            InitializeComponent();

			this.Load += (object sender, EventArgs e) => ChangedToTab(0);
        }

		private void OnError(object sender, GuiCommandEventArgs e)
		{
			int errorCode = e.Command.GetArgumentValue<int>("errorCode");
			string message = e.Command.GetArgumentValue<string>("message");
			switch (errorCode)
			{
				default:
					MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					break;
				//case (int)Commands.ErrorCode.NOT_AUTHORIZED:
				//	Provider.Handler.SendRegister();
				//	break;
			}
		}

        private void amazonS3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new CloudStorageAccountsForm())
			{
				form.ShowDialog(this);
			}
        }

        private void backupPlansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var presenter = new NewBackupPlanPresenter())
			{
				presenter.ShowDialog(this);
			}
			backupPlanListControl1.RefreshPlans();
			// Focusing is needed in some cases to avoid the mouse scrolling to stops working.
			// One case I confirmed is after going through all the `NewBackupPlanPresenter` process.
			backupPlanListControl1.Focus();
        }

		private void restorePlansToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var presenter = new NewRestorePlanPresenter())
			{
				presenter.ShowDialog(this);
			}
			restorePlanListControl1.RefreshPlans();
			// Focusing is needed in some cases to avoid the mouse scrolling to stops working.
			// One case I confirmed is after going through all the `NewRestorePlanPresenter` process.
			restorePlanListControl1.Focus();
		}

		private void ChangedToTab(int tabPageIndex)
		{
			switch (tabPageIndex)
			{
				case 0:
					backupPlanListControl1.LoadPlans();
					break;
				case 1:
					restorePlanListControl1.LoadPlans();
					break;
			}
		}

		private void tabControl1_Selected(object sender, TabControlEventArgs e)
		{
			ChangedToTab(e.TabPageIndex);
		}

		private void synchronizeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var presenter = new SyncPresenter())
			{
				presenter.ShowDialog(this);
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new AboutForm())
			{
				form.ShowDialog(this);
			}
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new SettingsForm())
			{
				form.ShowDialog(this);
			}
		}

		private void networkCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new NetworkCredentialsForm())
			{
				form.ShowDialog(this);
			}
		}

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
				DetachEventHandlers();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
