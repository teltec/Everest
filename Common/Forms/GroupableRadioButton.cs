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

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
		}

		[
		//Bindable(true),
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
				_RadioGroup = value;
				_RadioGroup.AddRadioButton(this);
			}
		}
	}
}
