/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teltec.Everest.Data.DAO;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.NetworkCredentials
{
	public partial class NetworkCredentialsForm : Form
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public NetworkCredentialsForm()
		{
			InitializeComponent();

			LoadInitialData();
		}

		private void LoadInitialData()
		{
			NetworkCredentialRepository dao = new NetworkCredentialRepository();
			DataGridDataSource.DataSource = All = dao.GetAll();
			RefreshDataGrid();
		}

		private void RefreshDataGrid()
		{
			DataGridDataSource.ResetBindings(false);
		}

		private List<Models.NetworkCredential> All = new List<Models.NetworkCredential>();
		private List<Models.NetworkCredential> Added = new List<Models.NetworkCredential>();
		private List<Models.NetworkCredential> Modified = new List<Models.NetworkCredential>();
		private List<Models.NetworkCredential> Removed = new List<Models.NetworkCredential>();

		private void dgvCredentials_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				DataGridView.HitTestInfo hit = dgvCredentials.HitTest(e.X, e.Y);
				if (hit.Type == DataGridViewHitTestType.Cell)
				{
					DataGridViewRow clickedRow = dgvCredentials.Rows[hit.RowIndex];
					//DataGridViewCell clickedCell = clickedRow.Cells[hit.ColumnIndex];
					EditRow(clickedRow);
				}
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			Models.NetworkCredential credential = new Models.NetworkCredential();
			using (var form = new AddEditNetworkCredentialForm(credential))
			{
				form.Canceled += (object sender2, NetworkCredentialActionEventArgs e2) =>
				{
					// Do nothing.
				};

				form.Confirmed += (object sender2, NetworkCredentialActionEventArgs e2) =>
				{
					Added.Add(e2.Credential);
					All.Add(e2.Credential);
					RefreshDataGrid();
				};

				form.ShowDialog(this);
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in this.dgvCredentials.SelectedRows)
			{
				EditRow(row);
			}
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in this.dgvCredentials.SelectedRows)
			{
				Models.NetworkCredential credential = (Models.NetworkCredential)row.DataBoundItem;

				bool isNew = Added.Contains(credential);
				if (isNew)
					Added.Remove(credential);
				else
					Removed.Add(credential);

				if (Modified.Contains(credential))
					Modified.Remove(credential);

				dgvCredentials.Rows.RemoveAt(row.Index);
				All.Remove(credential);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			// IMPORTANT: Reload all credentials so we don't show stale data the next time
			//            this window is opened.
			NetworkCredentialRepository dao = new NetworkCredentialRepository();
			foreach (Models.NetworkCredential cred in All)
			{
				dao.Refresh(cred);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			SaveChanges();
			Close();
		}

		private void SaveChanges()
		{
			NetworkCredentialRepository dao = new NetworkCredentialRepository();

			foreach (Models.NetworkCredential cred in Added)
			{
				logger.Debug("ADDED: login={0} mount={1} path={2}", cred.Login, cred.MountPoint, cred.Path);
				dao.Insert(cred);
			}

			foreach (Models.NetworkCredential cred in Modified)
			{
				logger.Debug("MODIFIED: login={0} mount={1} path={2}", cred.Login, cred.MountPoint, cred.Path);
				dao.Update(cred);
			}

			foreach (Models.NetworkCredential cred in Removed)
			{
				logger.Debug("REMOVED: login={0} mount={1} path={2}", cred.Login, cred.MountPoint, cred.Path);
				dao.Delete(cred);
			}
		}

		private void EditRow(DataGridViewRow row)
		{
			Models.NetworkCredential credential = (Models.NetworkCredential)row.DataBoundItem;
			Models.NetworkCredential untouchedCredential = credential.Clone() as Models.NetworkCredential;

			using (var form = new AddEditNetworkCredentialForm(credential))
			{
				form.Canceled += (object sender2, NetworkCredentialActionEventArgs e2) =>
				{
					// Revert changes.
					credential.RevertTo(untouchedCredential);
				};

				form.Confirmed += (object sender2, NetworkCredentialActionEventArgs e2) =>
				{
					bool isNew = Added.Contains(credential);
					if (!isNew)
					{
						Modified.Add(e2.Credential);
						dgvCredentials.InvalidateRow(row.Index);
					}
				};

				form.ShowDialog(this);
			}
		}
	}
}
