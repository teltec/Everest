using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;
using Teltec.Storage;

namespace Teltec.Backup.App.Forms.Settings
{
	public partial class SettingsForm : Form
	{
		public SettingsForm()
		{
			InitializeComponent();
			LoadSettings();
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			CleanForm();
			Apply();
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Cancel();
			Close();
		}

		private void CleanForm()
		{
		}

		private void LoadSettings()
		{
			//nudMaxThreads.Value = AsyncHelper.SettingsMaxThreadCount;
			nudMaxThreads.Value = Teltec.Backup.Settings.Properties.Current.MaxThreadCount;
		}

		private void SaveSettings()
		{
			int maxThreadCount = int.Parse(nudMaxThreads.Value.ToString());
			Teltec.Backup.Settings.Properties.Current.MaxThreadCount = maxThreadCount;

			Teltec.Backup.Settings.Properties.Save();

			AsyncHelper.SettingsMaxThreadCount = maxThreadCount;
		}

		private void Cancel()
		{
		}

		private void Apply()
		{
			SaveSettings();
		}
	}
}
