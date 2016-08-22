using System;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Common.Controls
{
	public partial class TextBoxSelectFolderDialog : UserControl
	{
		public TextBoxSelectFolderDialog()
		{
			InitializeComponent();

			folderBrowserDialog1.RootFolder = _RootFolder;
			folderBrowserDialog1.SelectedPath = "";

			// Bindings.
			tbPath.DataBindings.Add(new Binding("Text", this,
				this.GetPropertyName((TextBoxSelectFolderDialog x) => x.Text), false, DataSourceUpdateMode.OnPropertyChanged));
		}

		private Environment.SpecialFolder _RootFolder = Environment.SpecialFolder.MyComputer;
		public Environment.SpecialFolder RootFolder
		{
			get
			{
				return _RootFolder;
			}
			set
			{
				_RootFolder = value;
				folderBrowserDialog1.RootFolder = value;
			}
		}

		private void tbPath_Enter(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(tbPath.Text))
			{
				//LetUserSelectFolder();
			}
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			LetUserSelectFolder();
		}

		private bool LetUserSelectFolder()
		{
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				tbPath.Text = folderBrowserDialog1.SelectedPath;
				return true;
			}
			return false;
		}
	}
}
