/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Everest.App.Controls
{
	public partial class DriveItemsComboBox : ComboBox
	{
		public DriveItemsComboBox()
		{
			InitializeComponent();
		}

		[AttributeProvider(typeof(IListSource))]
		[DefaultValue("")]
		[RefreshProperties(RefreshProperties.Repaint)]
		public new DriveItemsBindingSource DataSource
		{
			get { return base.DataSource as DriveItemsBindingSource; }
			set { base.DataSource = value as DriveItemsBindingSource; }
		}

		protected override void OnDataSourceChanged(EventArgs e)
		{
			base.OnDataSourceChanged(e);

			if (DataSource != null)
				DataSource.DataSourceChanged += DataSource_DataSourceChanged;
		}

		void DataSource_DataSourceChanged(object sender, EventArgs e)
		{
			if (DataSource == null)
			{
				DisplayMember = null;
				ValueMember = null;
			}
			else
			{
				DisplayMember = this.GetPropertyName((DriveItem x) => x.Text);
				ValueMember = this.GetPropertyName((DriveItem x) => x.LocalDrive);
			}
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			this_DrawItem(this, e);
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			this_SelectedIndexChanged(this, e);
		}

		protected virtual bool IsItemEnabled(int rowIndex)
		{
			DriveItem item = (DriveItem)this.Items[rowIndex];
			return item.IsDriveAvailable;
		}

		#region Disabling items

		//
		// "Disabling particular Items in a Combobox" by "user276648" is licensed under CC BY-SA 3.0
		//
		// Title?   Disabling particular Items in a Combobox
		// Author?  user276648 - http://stackoverflow.com/users/276648/user276648
		// Source?  http://stackoverflow.com/a/15824758/298054
		// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
		//

		private void this_DrawItem(object sender, DrawItemEventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			object item = comboBox.Items[e.Index];

			if (IsItemEnabled(e.Index))
			{
				e.DrawBackground();
				// Set the brush according to whether the item is selected or not.
				bool isSelected = (e.State & DrawItemState.Selected) > 0;
				Brush brush = isSelected ? SystemBrushes.HighlightText : SystemBrushes.ControlText;
				e.Graphics.DrawString(item.ToString(), comboBox.Font, brush, e.Bounds);
				e.DrawFocusRectangle();
			}
			else
			{
				// NOTE we must draw the background or else each time we hover over the text it will be redrawn and its color will get darker and darker.
				e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
				e.Graphics.DrawString(item.ToString(), comboBox.Font, SystemBrushes.GrayText, e.Bounds);
			}
		}

		void this_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (!IsItemEnabled(this.SelectedIndex))
			//	this.SelectedIndex = -1;
		}

		#endregion

		#region Dispose Pattern Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
