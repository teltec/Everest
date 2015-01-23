using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Backup.Forms.BackupPlan;

namespace Teltec.Backup.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void amazonS3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new CloudStorageAccountsForm();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void backupPlansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var presenter = new NewBackupPlanPresenter();
			presenter.ShowDialog(this);
			presenter.Dispose();
        }
    }
}
