using System;
using System.Windows.Forms;

namespace Teltec.Backup.App.Controls
{
	public partial class TextBoxOpenFileDialog : UserControl
	{
		public TextBoxOpenFileDialog()
		{
			InitializeComponent();

			openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			// TODO(jweyrich): At some point, handle OpenFileDialog filters for binary files on other platforms as well.
			openFileDialog1.Filter = "Executable files (*.exe)|Batch files (*.bat)";
		}

		private void tbPath_Enter(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(tbPath.Text))
			{
				LetUserSelectFile();
			}
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			LetUserSelectFile();
		}

		private bool LetUserSelectFile()
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				openFileDialog1.FilterIndex = 0;
				openFileDialog1.RestoreDirectory = true;
				tbPath.Text = openFileDialog1.FileName;
				return true;
			}
			return false;
		}
	}
}
