using System;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Controls
{
	public partial class TextBoxOpenFileDialog : UserControl
	{
		public TextBoxOpenFileDialog()
		{
			InitializeComponent();

			openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			openFileDialog1.FileName = "";
			// TODO(jweyrich): At some point, handle OpenFileDialog filters for binary files on other platforms as well.
			openFileDialog1.Filter = "All supported files (*.exe, *.bat)|*.exe;*.bat|Executable files (*.exe)|*.exe|Batch files (*.bat)|*.bat";

			// Bindings.
			tbPath.DataBindings.Add(new Binding("Text", this,
				this.GetPropertyName((TextBoxOpenFileDialog x) => x.Text), false, DataSourceUpdateMode.OnPropertyChanged));
		}

		private void tbPath_Enter(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(tbPath.Text))
			{
				//LetUserSelectFile();
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
