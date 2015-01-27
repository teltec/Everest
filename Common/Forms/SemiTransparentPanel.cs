using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teltec.Common.Forms
{
	//
	// REFERENCES:
	//	http://stackoverflow.com/a/3724365/298054
	//	http://stackoverflow.com/a/9359642/298054
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
