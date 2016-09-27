/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Everest.App.Forms.S3;
using Teltec.Everest.Data.DAO;
using Teltec.Common.Extensions;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.BackupPlan
{
	public partial class BackupPlanSelectAccountForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly AmazonS3AccountRepository _s3dao = new AmazonS3AccountRepository();
		private Models.BackupPlan Plan = new Models.BackupPlan();

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

		public BackupPlanSelectAccountForm()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.BackupPlan;

				switch (this.Plan.StorageAccountType)
				{
					case Models.EStorageAccountType.AmazonS3:
						rbtnAmazonS3.Checked = true;
						break;
					case Models.EStorageAccountType.FileSystem:
						rbtnFileSystem.Checked = true;
						break;
				}

				if (this.Plan.StorageAccountType != Models.EStorageAccountType.Unknown)
				{
					LoadAccounts(this.Plan.StorageAccountType);

					if (this.Plan.StorageAccount != null)
						SelectExistingAccount(this.Plan.StorageAccountType, this.Plan.StorageAccount.Id);
				}
			};

			// Setup data bindings
			cbAmazonS3.DataBindings.Add(new Binding("Enabled", rbtnAmazonS3,
				this.GetPropertyName((RadioButton x) => x.Checked)));
			cbFileSystem.DataBindings.Add(new Binding("Enabled", rbtnFileSystem,
				this.GetPropertyName((RadioButton x) => x.Checked)));
		}

		protected override bool IsValid()
		{
			bool didSelectAccountType = rbtnAmazonS3.Checked || rbtnFileSystem.Checked;

			bool didSelectAccount = false;
			if (rbtnAmazonS3.Checked)
				didSelectAccount = cbAmazonS3.SelectedIndex > 0;
			else if (rbtnFileSystem.Checked)
				didSelectAccount = cbFileSystem.SelectedIndex > 0;

			return didSelectAccountType && didSelectAccount;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, select an account.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}

		private void LoadAccounts(Models.EStorageAccountType accountType, bool reload = false)
		{
			switch (accountType)
			{
				case Models.EStorageAccountType.AmazonS3:
					{
						if (!reload && this.cbAmazonS3.Items.Count > 0)
							return;

						var accounts = _s3dao.GetAll();
						accounts.Insert(0, new Models.AmazonS3Account() { DisplayName = "<Create new account>" });

						this.cbAmazonS3.DisplayMember = this.GetPropertyName((Models.AmazonS3Account x) => x.DisplayName);
						this.cbAmazonS3.ValueMember = this.GetPropertyName((Models.AmazonS3Account x) => x.Id);
						this.cbAmazonS3.DataSource = accounts;
						break;
					}
				case Models.EStorageAccountType.FileSystem:
					{
						if (!reload && this.cbFileSystem.Items.Count > 0)
							return;
						//var accounts = null;
						//accounts.Insert(0, new FileSystemAccount() { DisplayName = "<Create new account>" });
						break;
					}
			}
		}

		private void SelectExistingAccount(Models.EStorageAccountType accountType, int? accountId)
		{
			switch (accountType)
			{
				case Models.EStorageAccountType.AmazonS3:
					{
						this.cbAmazonS3.SelectedValue = accountId;
						break;
					}
				case Models.EStorageAccountType.FileSystem:
					{
						this.cbFileSystem.SelectedValue = accountId;
						break;
					}
			}
		}

		private void cbAmazonS3_DropDown(object sender, EventArgs e)
		{
			LoadAccounts(Models.EStorageAccountType.AmazonS3);
		}

		private void cbAmazonS3_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (cbAmazonS3.SelectedIndex == 0)
			{
				using (var form = new AmazonS3AccountForm(new Models.AmazonS3Account()))
				{
					form.AccountSaved += (object sender1, AmazonS3AccountSaveEventArgs e1) =>
					{
						_s3dao.Insert(e1.Account); // Insert new account into the database.
						LoadAccounts(Models.EStorageAccountType.AmazonS3, true);
						SelectExistingAccount(Models.EStorageAccountType.AmazonS3, e1.Account.Id);
					};
					form.AccountCanceled += (object sender1, AmazonS3AccountSaveEventArgs e1) =>
					{
						cbAmazonS3.SelectedIndex = -1; // Deselect it.
					};
					form.ShowDialog(this);
				}
			}

			if (cbAmazonS3.SelectedIndex != -1)
			{
				Models.BackupPlan plan = Model as Models.BackupPlan;
				plan.StorageAccountType = Models.EStorageAccountType.AmazonS3;
				plan.StorageAccount = _s3dao.Get((int)cbAmazonS3.SelectedValue);
			}
		}

		private void cbFileSystem_DropDown(object sender, EventArgs e)
		{
			if (this.cbFileSystem.Items.Count > 0)
				return;

			//var accounts = _s3fs.GetAll();
			//accounts.Insert(0, new FileSystemAccount() { DisplayName = "<Create new account>" });

			//this.cbFileSystem.DisplayMember = this.GetPropertyName((FileSystemAccount x) => x.DisplayName);
			//this.cbFileSystem.ValueMember = this.GetPropertyName((FileSystemAccount x) => x.Id);
			//this.cbFileSystem.DataSource = accounts;
		}

		private void cbFileSystem_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (cbFileSystem.SelectedIndex == 0)
			{
				MessageBox.Show("Show <Create new account> window.");
			}
			else
			{
				Models.BackupPlan plan = Model as Models.BackupPlan;
				plan.StorageAccountType = Models.EStorageAccountType.FileSystem;
				//plan.StorageAccount = new CloudStorageAccount { Id = (int)cbFileSystem.SelectedValue };
				throw new NotImplementedException();
			}
		}

	}
}
