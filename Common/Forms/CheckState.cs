using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Common.Forms
{
	// Our replacement for System.Windows.Forms.CheckState.Indeterminate
	public enum CheckState
	{
		Unchecked = 1,
		Checked = 2,
		Mixed = CheckState.Unchecked | CheckState.Checked,
	}
}
