using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Teltec.Common.Controls
{
	public partial class AdvancedTreeView : TreeView
	{
		public AdvancedTreeView()
		{
			UseWaitCursor = true;

			InitializeComponent();
			CreateStateImages();

			UseWaitCursor = false;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			Action<TreeNode> RecursivelySetStateImage = null;
			// Recursively set the checkbox images according to nodes states.
			RecursivelySetStateImage = new Action<TreeNode>(node =>
			{
				SetStateImage(node, node.ImageIndex);
				foreach (TreeNode n in node.Nodes)
					RecursivelySetStateImage(n);
				if (AutoExpandMixedNodes && node.ImageIndex == (int)CheckState.Mixed)
					node.Expand();
			});

			foreach (TreeNode n in Nodes)
				RecursivelySetStateImage(n);
		}

		private void CreateStateImages()
		{
			Action<CheckBoxState> AddToImageList = new Action<CheckBoxState>(state =>
			{
				Bitmap image = new Bitmap(16, 16);
				using (Graphics gfx = Graphics.FromImage(image))
					CheckBoxRenderer.DrawCheckBox(gfx, new Point(0, 0), state);
				this.StateImageList.Images.Add(image);
			});

			this.SuspendLayout();
			AddToImageList(CheckBoxState.UncheckedNormal); // 0
			AddToImageList(CheckBoxState.CheckedNormal);   // 1
			AddToImageList(CheckBoxState.MixedNormal);     // 2
			this.ResumeLayout();
		}

		protected void SetStateImage(TreeNode node, int index)
		{
			if (!IsHandleCreated)
				return;
			if (node == null)
				throw new ArgumentNullException("node", "Node can not be null");
			if (node.Handle == null)
				return;
			if (index < 0)
				//throw new ArgumentException("Index must be greater than zero.", "index");
				index = (int)CheckState.Unchecked;

			node.ImageIndex = index;

			NativeMethods.TV_ITEM item = new NativeMethods.TV_ITEM();
			item.mask = NativeMethods.TVIF_HANDLE | NativeMethods.TVIF_STATE;
			item.hItem = node.Handle;
			item.stateMask = NativeMethods.TVIS_STATEIMAGEMASK;
			item.state |= index << 12;
			NativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.TVM_SETITEM, IntPtr.Zero, ref item);
		}

		public CheckState GetCheckState(TreeNode node)
		{
			return node.ImageIndex < 0 ? CheckState.Unchecked : (CheckState)node.ImageIndex;
		}

		public void SetCheckState(TreeNode node, CheckState state)
		{
			if (!InternalSetCheckedState(node, state))
				return;
			CheckNode(node, state);
			UpdateParentNode(node.Parent);
		}

		protected void UncheckNodeSiblings(TreeNode node)
		{
			// Update previous sibling nodes.
			TreeNode prevNode = node.PrevNode;
			while (prevNode != null)
			{
				InternalSetCheckedState(prevNode, CheckState.Unchecked);
				prevNode = prevNode.PrevNode;
			}

			// Update next sibling nodes.
			TreeNode nextNode = node.NextNode;
			while (nextNode != null)
			{
				InternalSetCheckedState(nextNode, CheckState.Unchecked);
				nextNode = nextNode.NextNode;
			}
		}

		protected virtual void CheckNode(TreeNode node, CheckState state)
		{
			InternalSetCheckedState(node, state);

			// Update child nodes.
			foreach (TreeNode child in node.Nodes)
				CheckNode(child, CheckState.Unchecked);
		}

		private void UpdateParentNode(TreeNode node)
		{
			if (node == null)
				return;

			CheckState childStates = 0;
			foreach (TreeNode child in node.Nodes)
				childStates |= GetCheckState(child);

			CheckState newParentState = childStates == CheckState.Unchecked
				? CheckState.Unchecked
				: CheckState.Mixed;

			if (InternalSetCheckedState(node, newParentState))
				UpdateParentNode(node.Parent);
		}

		private bool InternalSetCheckedState(TreeNode node, CheckState state)
		{
			TreeViewCancelEventArgs args = new TreeViewCancelEventArgs(node, false, TreeViewAction.Unknown);
			OnBeforeCheck(args);
			if (args.Cancel)
				return false;

			SetStateImage(node, (int)state);

			OnAfterCheck(new TreeViewEventArgs(node, TreeViewAction.Unknown));
			return true;
		}

		protected void ChangeNodeState(TreeNode node)
		{
			BeginUpdate();
			CheckState newState;
			//if (node.ImageIndex == (int)CheckState.Unchecked || node.ImageIndex < 0)
			if (node.ImageIndex != (int)CheckState.Checked)
				newState = CheckState.Checked;
			else
				newState = CheckState.Unchecked;
			CheckNode(node, newState);
			UpdateParentNode(node.Parent);
			EndUpdate();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			Point pt = PointToClient(Control.MousePosition);

			TreeViewHitTestInfo info = this.HitTest(pt);
			TreeViewHitTestLocations loc = info.Location;
			// Is the click on the checkbox? If not, ignore it.
			if (loc == TreeViewHitTestLocations.StateImage)
			{
				if (info.Node != null)
					ChangeNodeState(info.Node);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Space)
				ChangeNodeState(SelectedNode);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case NativeMethods.WM_LBUTTONDBLCLK:
					{
						//
						// "Winforms treeview, recursively check child nodes problem" by "Hans Passant" is licensed under CC BY-SA 3.0
						//
						// Title?   Winforms treeview, recursively check child nodes problem
						// Author?  Hans Passant - http://stackoverflow.com/users/17034/hans-passant
						// Source?  http://stackoverflow.com/a/3174824/298054
						// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
						//

						//
						// Disable double-click on checkbox to fix Windows Vista/7/8/8.1 bug.
						//
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null && hitInfo.Location == TreeViewHitTestLocations.StateImage)
						{
							m.Result = IntPtr.Zero;
							return;
						}
						break;
					}
				case NativeMethods.WM_RBUTTONDOWN:
					{
						//
						// ORIGINAL CODE FROM:
						//	https://social.msdn.microsoft.com/Forums/windows/en-US/9d717ce0-ec6b-4758-a357-6bb55591f956/possible-bug-in-net-treeview-treenode-checked-state-inconsistent?forum=winforms
						//

						//
						// Fix for another Microsoft bug.
						// Right-clicking a node doesn't make it the selected node.
						//
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null)
							this.SelectedNode = hitInfo.Node;
						break;
					}
			}

			base.WndProc(ref m);
		}

		protected void ShowErrorMessage(Exception exception, TreeNode node)
		{
			MessageBox.Show(this, exception.Message, "Error");
			if (node != null)
			{
				SetCheckState(node, CheckState.Unchecked);
				node.Collapse();
			}
		}

		#region Custom properties

		[
		Category("Custom"),
		DefaultValue(true),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		public bool _AutoExpandMixedNodes = true;
		public bool AutoExpandMixedNodes
		{
			get { return _AutoExpandMixedNodes; }
			set { _AutoExpandMixedNodes = value; }
		}

		#endregion

		#region Custom events

		public delegate void ExpandFetchStartedHandler(object sender, EventArgs e);
		public delegate void ExpandFetchEndedHandler(object sender, EventArgs e);

		public virtual event ExpandFetchStartedHandler ExpandFetchStarted;
		public virtual event ExpandFetchEndedHandler ExpandFetchEnded;

		protected virtual void OnExpandFetchStarted(object sender, EventArgs e)
		{
			if (ExpandFetchStarted != null)
				ExpandFetchStarted(this, e);
		}

		protected virtual void OnExpandFetchEnded(object sender, EventArgs e)
		{
			if (ExpandFetchEnded != null)
				ExpandFetchEnded(this, e);
		}

		#endregion

		#region Hide some properties from the Designer

		[Browsable(false)]
		public new bool CheckBoxes
		{
			get { return base.CheckBoxes; }
			set { base.CheckBoxes = value; }
		}

		[Browsable(false)]
		public new ImageList ImageList
		{
			get { return base.ImageList; }
			set { base.ImageList = value; }
		}

		[Browsable(false)]
		public new string ImageKey
		{
			get { return base.ImageKey; }
			set { base.ImageKey = value; }
		}

		[Browsable(false)]
		public new int ImageIndex
		{
			get { return base.ImageIndex; }
			set { base.ImageIndex = value; }
		}

		[Browsable(false)]
		public new int SelectedImageIndex
		{
			get { return base.SelectedImageIndex; }
			set { base.SelectedImageIndex = value; }
		}

		[Browsable(false)]
		public new string SelectedImageKey
		{
			get { return base.SelectedImageKey; }
			set { base.SelectedImageKey = value; }
		}

		[Browsable(false)]
		public new ImageList StateImageList
		{
			get { return base.StateImageList; }
			set { base.StateImageList = value; }
		}

		#endregion
	}
}
