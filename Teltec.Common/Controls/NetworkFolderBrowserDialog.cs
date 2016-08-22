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
