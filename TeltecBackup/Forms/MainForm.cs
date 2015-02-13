using NLog;
using System;
using System.Windows.Forms;
using Teltec.Backup.DAO;
using Teltec.Backup.Forms.BackupPlan;

namespace Teltec.Backup.Forms
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
    }
}
