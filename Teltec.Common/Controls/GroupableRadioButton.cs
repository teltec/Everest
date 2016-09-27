/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	public partial class GroupableRadioButton : RadioButton
	{
		public GroupableRadioButton()
		{
			InitializeComponent();
		}

		[
		Category("Misc"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected RadioButtonGroup _RadioGroup;
		public RadioButtonGroup RadioGroup
		{
			get { return _RadioGroup; }
			//set { SetField(ref _RadioGroup, value); }
			set {
				// Did not change?
				if (_RadioGroup == value)
					return;

				// Adding to a group?
				if (value != null)
				{
					// Has previous group?
					if (_RadioGroup != null)
						_RadioGroup.RemoveRadioButton(this);
					value.AddRadioButton(this);
				}
				else // Removing from a group?
				{
					// Has previous group?
					if (_RadioGroup != null)
						_RadioGroup.RemoveRadioButton(this);
				}
				_RadioGroup = value;
			}
		}
	}
}
