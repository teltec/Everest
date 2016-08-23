using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Teltec.Everest.App.Forms.Account
{
	public partial class AccountConfigurationSelector : Form
	{
		public List<string> AvailableConfigurations;
		public string SelectedConfiguration;

		public AccountConfigurationSelector()
		{
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			cbExistingConfigurations.Items.AddRange(AvailableConfigurations.ToArray());

			int index = AvailableConfigurations.FindIndex(x => x.Equals(CurrentAccountConfigurationName, StringComparison.InvariantCulture));
			if (index >= 0)
				cbExistingConfigurations.SelectedIndex = index;
		}

		private string CurrentAccountConfigurationName
		{
			get { return SelectedConfiguration != null ? SelectedConfiguration : DefaultConfigurationName; }
		}

		private string DefaultConfigurationName
		{
			get { return Environment.MachineName; }
		}

		private string GetUniqueConfigurationName(string name)
		{
			bool alreadyContains = AvailableConfigurations.Contains(name);
			if (!alreadyContains)
				return name;

			for (int i = 1; i < int.MaxValue; i++)
			{
				alreadyContains = AvailableConfigurations.Contains(name + "_" + i);
				if (!alreadyContains)
					return name + "_" + i;
			}

			throw new InvalidOperationException("That's bad! The range of possible configuration names has been exausted.");
		}

		private void btnConfirm_Click(object sender, EventArgs e)
		{
			int index = cbExistingConfigurations.SelectedIndex;

			if (!cbCreateNew.Checked && index < 0)
			{
				MessageBox.Show("Please, select an existing configuration\n"
					+ "or mark the checkbox below to create a new one.",
					"Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			SelectedConfiguration = cbCreateNew.Checked
				? GetUniqueConfigurationName(DefaultConfigurationName)
				: AvailableConfigurations.ElementAt(index);

			this.Close();
		}

		private void cbCreateNew_CheckedChanged(object sender, EventArgs e)
		{
			cbExistingConfigurations.Enabled = !cbCreateNew.Checked;
		}
	}
}
