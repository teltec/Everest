﻿/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Everest.Data.DAO;
using Teltec.Common.Extensions;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.RestorePlan
{
	public partial class RestorePlanSelectAccountForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly AmazonS3AccountRepository _s3dao = new AmazonS3AccountRepository();
		private Models.RestorePlan Plan = new Models.RestorePlan();

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

		public RestorePlanSelectAccountForm()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.RestorePlan;

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

		private void LoadAccounts(Models.EStorageAccountType accountType)
		{
			switch (accountType)
			{
				case Models.EStorageAccountType.AmazonS3:
					{
						if (this.cbAmazonS3.Items.Count > 0)
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
						if (this.cbFileSystem.Items.Count > 0)
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
				MessageBox.Show("Show <Create new account> window.");
			}
			else
			{
				Models.RestorePlan plan = Model as Models.RestorePlan;
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
				Models.RestorePlan plan = Model as Models.RestorePlan;
				plan.StorageAccountType = Models.EStorageAccountType.FileSystem;
				//plan.StorageAccount = new CloudStorageAccount { Id = (int)cbFileSystem.SelectedValue };
				throw new NotImplementedException();
			}
		}

	}
}