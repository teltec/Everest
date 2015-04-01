
namespace Teltec.Common.Controls
{
	// Our replacement for System.Windows.Forms.CheckState.Indeterminate
	public enum CheckState
	{
		Unchecked = 1,
		Checked = 2,
		Mixed = CheckState.Unchecked | CheckState.Checked,
	}
}
