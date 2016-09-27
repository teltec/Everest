/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Reflection;
using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	public partial class NetworkFolderBrowserDialog : UserControl
	{
		public NetworkFolderBrowserDialog()
		{
			InitializeComponent();
		}

		private void ChangeDialogInternalBehavior(FolderBrowserDialog dialog)
		{
			Type type = dialog.GetType();
			FieldInfo fieldInfo = type.GetField("rootFolder", BindingFlags.NonPublic | BindingFlags.Instance);

			Environment.SpecialFolder networkNeighborhood = (Environment.SpecialFolder)18;
			fieldInfo.SetValue(dialog, networkNeighborhood);
		}

		public string GetNetworkFolders()
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				ChangeDialogInternalBehavior(dialog);
				DialogResult ret = dialog.ShowDialog();
				return ret == DialogResult.OK
					? dialog.SelectedPath.ToString()
					: string.Empty;
			}
		}
	}
}
