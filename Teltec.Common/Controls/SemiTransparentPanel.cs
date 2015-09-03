using System.Drawing;
using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	//
	// "Winforms: Making a control transparent" by "Amen Ayach" is licensed under CC BY-SA 3.0
	//
	// Title?   Winforms: Making a control transparent
	// Author?  Amen Ayach - http://stackoverflow.com/users/1209153/amen-ayach
	// Source?  http://stackoverflow.com/a/9359642/298054
	// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
	//
	public partial class SemiTransparentPanel : Panel
	{
		public SemiTransparentPanel()
		{
			InitializeComponent();
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			//this.SetStyle(ControlStyles.Opaque, true);
			this.BackColor = Color.FromArgb(128, 0, 0, 0);
		}
	}
}
