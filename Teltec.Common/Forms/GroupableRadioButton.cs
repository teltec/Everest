using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teltec.Common.Forms
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
