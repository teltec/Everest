using System;
using System.Windows.Forms;
using Teltec.Backup.App.Controls;
using Teltec.Common.Controls;
using Teltec.Common.Extensions;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.NetworkCredentials
{
	public partial class AddEditNetworkCredentialForm : Form
	{
		public delegate void ActionEventHandler(object sender, NetworkCredentialActionEventArgs e);

		public event ActionEventHandler Canceled;
		public event ActionEventHandler Confirmed;

		private Models.NetworkCredential Model;

		public AddEditNetworkCredentialForm(Models.NetworkCredential credential)
		{
			InitializeComponent();

			Model = credential;

			drivesBindingSource1.BindData();

			WireBindings();
		}

		private void WireBindings()
		{
			txtNetworkPath.DataBindings.Add(new Binding("Text", this.Model,
				this.GetPropertyName((Models.NetworkCredential x) => x.Path)));

			cbMountPoint.DataBindings.Add(new Binding("SelectedItem", this.Model,
				this.GetPropertyName((Models.NetworkCredential x) => x.MountPoint)));

			txtLogin.DataBindings.Add(new Binding("Text", this.Model,
				this.GetPropertyName((Models.NetworkCredential x) => x.Login)));

			txtPassword.DataBindings.Add(new Binding("Text", this.Model,
				this.GetPropertyName((Models.NetworkCredential x) => x.Password)));
		}

		private void btnOpenNetworkPathDialog_Click(object sender, EventArgs e)
		{
			NetworkFolderBrowserDialog dialog = new NetworkFolderBrowserDialog();
			string selectedPath = dialog.GetNetworkFolders();
			if (!string.IsNullOrEmpty(selectedPath))
				txtNetworkPath.Text = selectedPath;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			InvokeDelegate(Canceled, new NetworkCredentialActionEventArgs { Credential = Model });
		}

		private void cbMountPoint_SelectionChangeCommitted(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;

			int selIndex = comboBox.SelectedIndex;
			if (selIndex < 0 || selIndex > comboBox.Items.Count)
				return;

			DriveItem item = (DriveItem)cbMountPoint.Items[selIndex];
			if (item != null)
			{
				bool isMappedPath = !item.IsDriveAvailable && !string.IsNullOrEmpty(item.MappedPath);
				if (isMappedPath)
				{
					bool isSamePath = item.MappedPath.Trim().Equals(txtNetworkPath.Text.Trim());
					if (isSamePath)
						return;

					string caption = string.Format("{0} is already in use", item.LocalDrive);
					string message = string.Format(
						"The {0} unit is currently mounted to a different location."
						+ " Do you want to use the current active location?", item.LocalDrive);
					DialogResult dr = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dr == DialogResult.Yes)
						Model.Path = item.MappedPath;
				}
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			bool ok = DoValidate();
			if (!ok)
				return;

			InvokeDelegate(Confirmed, new NetworkCredentialActionEventArgs { Credential = Model });
			Close();
		}

		#region Validation

		private bool DoValidate()
		{
			if (IsValid())
				return true;

			MessageBox.Show("Invalid input provided. Please review your data.", "Invalid input",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		private bool IsValid()
		{
			bool hasPath = !string.IsNullOrEmpty(Model.Path);
			bool hasDrive = !string.IsNullOrEmpty(Model.MountPoint) && cbMountPoint.SelectedValue != null;
			bool hasLogin = !string.IsNullOrEmpty(Model.Login);
			bool hasPassword = !string.IsNullOrEmpty(Model.Password);

			return hasPath && hasDrive && hasLogin && hasPassword;
		}

		#endregion

		protected virtual void InvokeDelegate(Delegate method, EventArgs e)
		{
			if (method != null)
			{
				if (Owner.InvokeRequired)
					Owner.BeginInvoke(method, new object[] { this, e });
				else
					method.DynamicInvoke(this, e);
			}
		}
	}

	public class NetworkCredentialActionEventArgs : EventArgs
	{
		public Models.NetworkCredential Credential;
	}
}
