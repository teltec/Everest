/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using Teltec.Storage;

namespace Teltec.Everest.App.Forms.Settings
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
			nudMaxThreads.Value = Teltec.Everest.Settings.Properties.Current.MaxThreadCount;
			nudUploadChunkSize.Value = Teltec.Everest.Settings.Properties.Current.UploadChunkSize;
		}

		private void SaveSettings()
		{
			int maxThreadCount = int.Parse(nudMaxThreads.Value.ToString());
			Teltec.Everest.Settings.Properties.Current.MaxThreadCount = maxThreadCount;

			int uploadChunkSize = int.Parse(nudUploadChunkSize.Value.ToString());
			Teltec.Everest.Settings.Properties.Current.UploadChunkSize = uploadChunkSize;

			Teltec.Everest.Settings.Properties.Save();

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
