using NLog;
using System;
using System.Windows.Forms;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Forms.BackupPlan;
using Teltec.Backup.App.Forms.RestorePlan;

namespace Teltec.Backup.App.Forms
{
    public partial class MainForm : Form
    {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly BackupPlanRepository _dao = new BackupPlanRepository();

        public MainForm()
        {
            InitializeComponent();
			backupPlanListControl1.LoadPlans();
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
			//restorePlanListControl1.RefreshPlans();
			// Focusing is needed in some cases to avoid the mouse scrolling to stops working.
			// One case I confirmed is after going through all the `NewBackupPlanPresenter` process.
			//restorePlanListControl1.Focus();
		}
    }
}
