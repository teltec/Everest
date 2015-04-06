using System.Windows.Forms;

namespace Teltec.Backup.App.Controls
{
	public abstract class EntryTreeNode : TreeNode
	{
		public EntryTreeNode()
			: base()
		{
		}

		public EntryTreeNode(string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex)
		{
		}

		public abstract void OnExpand();
	}
}
