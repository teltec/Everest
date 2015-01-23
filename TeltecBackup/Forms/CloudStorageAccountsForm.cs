using NLog;
using System;
using System.Linq;
using System.Windows.Forms;
using Teltec.Backup.Forms.S3;
using Teltec.Backup.Models;

namespace Teltec.Backup.Forms
{
    public partial class CloudStorageAccountsForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private DbContextScope _dbContextScope = new DbContextScope();

        public CloudStorageAccountsForm()
        {
            InitializeComponent();
            logger.Debug("Created form {0}", this.GetType().Name);
        }

        protected void LoadAccounts()
        {
            this.lvAccounts.Items.Clear();

            //var query = from acc in _dbContextScope.AmazonS3Accounts.Objects
            //            where acc.DisplayName == "Jardel"
            //            select acc;
            var query = from acc in _dbContextScope.AmazonS3Accounts.Objects select acc;

            var accounts = query.ToList<AmazonS3Account>();
            foreach (var account in accounts)
            {
                ListViewItem item = new ListViewItem(account.DisplayName, 0);
                item.Tag = account.Id;
                this.lvAccounts.Items.Add(item);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadAccounts();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            bool hasSelection = this.lvAccounts.SelectedItems.Count > 0;

            foreach (ListViewItem item in this.lvAccounts.SelectedItems)
            {
                // Remove selected items from the list view.
                this.lvAccounts.Items.Remove(item);

                // Remove actual account model and persist?
                _dbContextScope.AmazonS3Accounts.Delete(item.Tag);
            }

            if (hasSelection)
                _dbContextScope.Save();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new AmazonS3AccountForm(new AmazonS3Account());
            form.AccountSaved += form_AccountSaved;
            form.AccountCancelled += form_AccountCancelled;
            form.ShowDialog(this);
            form.Dispose();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvAccounts.SelectedItems.Count == 0)
                return;

            var item = lvAccounts.SelectedItems[0];
            var selectedAcount = _dbContextScope.AmazonS3Accounts.Get(item.Tag);
            var form = new AmazonS3AccountForm(selectedAcount);
            form.AccountSaved += form_AccountChanged;
            form.AccountCancelled += form_AccountCancelled;
            form.ShowDialog(this);
            form.Dispose();
        }

        void form_AccountCancelled(object sender, AmazonS3AccountSaveEventArgs e)
        {
            _dbContextScope.AmazonS3Accounts.UndoChanges(e.Account);
        }

        void form_AccountSaved(object sender, AmazonS3AccountSaveEventArgs e)
        {
            e.Account.Id = Guid.NewGuid();
            _dbContextScope.AmazonS3Accounts.Insert(e.Account);
            _dbContextScope.Save();
            LoadAccounts();
        }

        void form_AccountChanged(object sender, AmazonS3AccountSaveEventArgs e)
        {
            _dbContextScope.AmazonS3Accounts.Update(e.Account);
            _dbContextScope.Save();
            LoadAccounts();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            _dbContextScope.Dispose();
            base.Dispose(disposing);
        }

        private void lvAccounts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

    }
}
