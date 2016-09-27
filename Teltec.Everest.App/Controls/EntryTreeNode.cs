/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;

namespace Teltec.Everest.App.Controls
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
