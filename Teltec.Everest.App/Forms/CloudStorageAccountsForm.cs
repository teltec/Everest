/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Windows.Forms;
using Teltec.Everest.App.Forms.S3;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms
{
    public partial class CloudStorageAccountsForm : Form
    {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly AmazonS3AccountRepository _s3dao = new AmazonS3AccountRepository();

        public CloudStorageAccountsForm()
        {
            InitializeComponent();
        }

        protected void LoadAccounts()
        {
            this.lvAccounts.Items.Clear();

			var accounts = _s3dao.GetAll();

			foreach (var account in accounts)
			{
				ListViewItem item = new ListViewItem(account.DisplayName, 0);
				item.Tag = account.Id;
				lvAccounts.Items.Add(item);
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
				_s3dao.Delete((int)item.Tag);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
			using (var form = new AmazonS3AccountForm(new AmazonS3Account()))
			{
				form.AccountSaved += form_AccountSaved;
				form.AccountCanceled += form_AccountCancelled;
				form.ShowDialog(this);
			}
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvAccounts.SelectedItems.Count == 0)
                return;

            var item = lvAccounts.SelectedItems[0];
            var selectedAcount = _s3dao.Get((int)item.Tag);
			using (var form = new AmazonS3AccountForm(selectedAcount))
			{
				form.AccountSaved += form_AccountChanged;
				form.AccountCanceled += form_AccountCancelled;
				form.ShowDialog(this);
            }
        }

        void form_AccountCancelled(object sender, AmazonS3AccountSaveEventArgs e)
        {
            _s3dao.Refresh(e.Account);
        }

        void form_AccountSaved(object sender, AmazonS3AccountSaveEventArgs e)
        {
            _s3dao.Insert(e.Account);
            LoadAccounts();
        }

        void form_AccountChanged(object sender, AmazonS3AccountSaveEventArgs e)
        {
			_s3dao.Update(e.Account);
            LoadAccounts();
        }

        private void lvAccounts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

		#region Dispose Pattern Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
    }
}
