/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Common.Controls
{
	public partial class TextBoxSelectFileDialog : UserControl
	{
		public TextBoxSelectFileDialog()
		{
			InitializeComponent();

			openFileDialog1.InitialDirectory = _InitialDirectory;
			openFileDialog1.FileName = "";
			openFileDialog1.Filter = _Filter;

			// Bindings.
			tbPath.DataBindings.Add(new Binding("Text", this,
				this.GetPropertyName((TextBoxSelectFileDialog x) => x.Text), false, DataSourceUpdateMode.OnPropertyChanged));
		}

		private string _InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		public string InitialDirectory
		{
			get
			{
				return _InitialDirectory;
			}
			set
			{
				_InitialDirectory = value;
				openFileDialog1.InitialDirectory = value;
			}
		}

		private string _Filter = "All files (*.*)|*.*";
		public string Filter
		{
			get
			{
				return _Filter;
			}
			set
			{
				_Filter = value;
				openFileDialog1.Filter = value;
			}
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
