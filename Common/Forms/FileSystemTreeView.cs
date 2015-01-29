using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Teltec.Common.Forms
{
	public partial class FileSystemTreeView : TreeView
	{
		public FileSystemTreeView()
		{
			UseWaitCursor = true;
			
			InitializeComponent();
			PopulateTreeView();
			
			UseWaitCursor = false;
			
			this.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.handle_BeforeExpand);
		}

		public enum CheckState
		{
			Unchecked = 1,
			Checked = 2,
			Mixed = CheckState.Unchecked | CheckState.Checked,
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			Action<CheckBoxState> AddToImageList = new Action<CheckBoxState>(state =>
			{
				Bitmap image = new Bitmap(16, 16);
				Graphics gfx = Graphics.FromImage(image);
				CheckBoxRenderer.DrawCheckBox(gfx, new Point(0, 0), state);
				this.stateImageList.Images.Add(image);
				gfx.Dispose();
			});

			AddToImageList(System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal); // 0 - Not used?
			AddToImageList(System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal); // 1
			AddToImageList(System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);   // 2
			AddToImageList(System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);     // 3

			SetStateImageList(stateImageList);
		}

		private void SetStateImageList(ImageList list)
		{
			if (list != null)
			{
				Message message = Message.Create(Handle, (int)Native.TVM_SETIMAGELIST, (IntPtr)Native.TVSIL_STATE, list.Handle);
				DefWndProc(ref message);
			}
		}

		internal void SetStateImage(TreeNode node, int index)
		{
			if (!IsHandleCreated)
				return;
			if (node == null)
				throw new ArgumentNullException("node", "Node can not be null");
			if (index < 0)
				throw new ArgumentException("index", "Index must be greater than zero.");
			if (node.Handle == null)
				return;

			Native.TVITEMEX tvi = new Native.TVITEMEX();
			tvi.mask = Native.TVIF_HANDLE | Native.TVIF_STATE;
			tvi.state = (uint)index;
			tvi.state = tvi.state << 12;
			tvi.stateMask = Native.TVIS_STATEIMAGEMASK;
			tvi.hItem = node.Handle;
			tvi.pszText = IntPtr.Zero;
			tvi.cchTextMax = 0;
			tvi.iImage = 0;
			tvi.iSelectedImage = 0;
			tvi.cChildren = 0;
			tvi.lParam = IntPtr.Zero;
			tvi.iIntegral = 0;

			Native.SendMessage(new HandleRef(this, Handle), Native.TVM_SETITEM, IntPtr.Zero, ref tvi);
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

		private void CheckNode(TreeNode node, CheckState state)
		{
			InternalSetCheckedState(node, state);
			foreach (TreeNode child in node.Nodes)
				CheckNode(child, state);
		}

		private void UpdateParentNode(TreeNode node)
		{
			if (node == null)
				return;

			CheckState state = GetCheckState(node.FirstNode);
			foreach (TreeNode child in node.Nodes)
				state |= GetCheckState(child);

			if (InternalSetCheckedState(node, state))
				UpdateParentNode(node.Parent);
		}

		private bool InternalSetCheckedState(TreeNode node, CheckState state)
		{
			TreeViewCancelEventArgs args = new TreeViewCancelEventArgs(node, false, TreeViewAction.Unknown);
			OnBeforeCheck(args);
			if (args.Cancel)
				return false;

			node.ImageIndex = (int)state;
			node.SelectedImageIndex = (int)state;
			SetStateImage(node, (int)state);

			OnAfterCheck(new TreeViewEventArgs(node, TreeViewAction.Unknown));
			return true;
		}

		protected void ChangeNodeState(TreeNode node)
		{
			BeginUpdate();
			CheckState newState;
			if (node.ImageIndex == (int)CheckState.Unchecked || node.ImageIndex < 0)
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
			//
			// REFERENCES:
			//	http://stackoverflow.com/a/3174824/298054
			//	https://social.msdn.microsoft.com/Forums/windows/en-US/9d717ce0-ec6b-4758-a357-6bb55591f956/possible-bug-in-net-treeview-treenode-checked-state-inconsistent?forum=winforms
			//

			switch (m.Msg)
			{
				case Native.WM_LBUTTONDBLCLK:
					{
						// Disable double-click on checkbox to fix Microsoft Vista bug.
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null && hitInfo.Location == TreeViewHitTestLocations.StateImage)
						{
							m.Result = IntPtr.Zero;
							return;
						}
						break;
					}
				case Native.WM_RBUTTONDOWN:
					{
						// Set focus to node on right-click - another Microsoft bug?
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null)
							this.SelectedNode = hitInfo.Node;
						break;
					}
			}

			base.WndProc(ref m);
		}

		#region Populate methods

		private void PopulateTreeView()
		{
			SuspendLayout();
			try
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				foreach (var drive in drives)
				{
					TreeNode driveNode = AddDriveNode(this, drive);
					AddLazyLoadingNode(driveNode);
				}
			}
			catch (System.SystemException e)
			{
				ShowErrorMessage(e, null);
			}
			ResumeLayout();
		}

		private void PopulateDrive(TreeNode parentNode, DriveInfo parentDrive)
		{
			PopuplateDirectory(parentNode, parentDrive.RootDirectory);
		}

		private void PopuplateDirectory(TreeNode parentNode, DirectoryInfo parentDir)
		{
			SuspendLayout();
			try
			{
				DirectoryInfo[] subDirs = parentDir.GetDirectories();
				FileInfo[] subFiles = parentDir.GetFiles();
				foreach (DirectoryInfo subDir in subDirs)
				{
					TreeNode subFolderNode = AddFolderNode(parentNode, subDir);
					AddLazyLoadingNode(subFolderNode);
				}
				foreach (var file in subFiles)
				{
					TreeNode subFileNode = AddFileNode(parentNode, file);
				}
			}
			catch (System.SystemException e)
			{
				ShowErrorMessage(e, parentNode);
			}
			ResumeLayout();
		}

		private void ShowErrorMessage(Exception exception, TreeNode node)
		{
			MessageBox.Show(this, exception.Message, "Error");
			if (node != null)
			{
				SetCheckState(node, CheckState.Unchecked);
				node.Collapse();
			}
		}

		private TreeNode AddDriveNode(TreeView view, DriveInfo drive)
		{
			String nodeName = null;
			try
			{
				string driveLabel = drive.VolumeLabel;
				if (string.IsNullOrEmpty(driveLabel))
					nodeName = drive.Name;
				else
					nodeName = String.Format("{0} ({1})", drive.Name, driveLabel);
			}
			catch (Exception)
			{
				nodeName = drive.Name;
			}
			TreeNode node = new TreeNode(nodeName, 0, 0);
			node.Tag = new TreeNodeTag(TreeNodeTag.InfoType.DRIVE, drive);
			node.ImageKey = "drive";
			view.Nodes.Add(node);
			return node;
		}

		private TreeNode AddFolderNode(TreeNode parentNode, DirectoryInfo folder)
		{
			TreeNode node = new TreeNode(folder.Name, 0, 0);
			node.Tag = new TreeNodeTag(TreeNodeTag.InfoType.FOLDER, folder);
			node.ImageKey = "folder";
			parentNode.Nodes.Add(node);
			if (GetCheckState(parentNode) == CheckState.Checked)
				SetCheckState(node, CheckState.Checked);
			return node;
		}

		private TreeNode AddFileNode(TreeNode parentNode, FileInfo file)
		{
			TreeNode node = new TreeNode(file.Name, 0, 0);
			node.Tag = new TreeNodeTag(TreeNodeTag.InfoType.FILE, file);
			node.ImageKey = "file";
			parentNode.Nodes.Add(node);
			if (GetCheckState(parentNode) == CheckState.Checked)
				SetCheckState(node, CheckState.Checked);
			return node;
		}

		private TreeNode AddLazyLoadingNode(TreeNode parentNode)
		{
			TreeNode node = new TreeNode("Retrieving data...", 0, 0);
			node.Tag = new TreeNodeTag(TreeNodeTag.InfoType.LOADING, null);
			node.ImageKey = "loading";
			parentNode.Nodes.Add(node);
			return node;
		}

		private void RemoveLazyLoadingNode(TreeNode parentNode)
		{
			TreeNode firstChildNode = parentNode.FirstNode;
			if (firstChildNode == null)
				return;

			TreeNodeTag tag = firstChildNode.Tag as TreeNodeTag;
			if (tag != null && tag.Type == TreeNodeTag.InfoType.LOADING)
				firstChildNode.Remove();
		}

		#endregion

		public class TreeNodeTag
		{
			public enum InfoType
			{
				LOADING = 0,
				DRIVE = 1,
				FOLDER = 2,
				FILE = 3,
			}

			public TreeNodeTag(InfoType infoType, object infoObject)
			{
				Type = infoType;
				InfoObject = infoObject;
			}

			public InfoType Type { get; private set; }
			public object InfoObject { get; private set; }

			public string Path
			{
				get
				{
					if (this.InfoObject == null)
						return null;

					switch (this.Type)
					{
						case InfoType.FILE:
							return (this.InfoObject as FileInfo).FullName;
						case InfoType.FOLDER:
							return (this.InfoObject as DirectoryInfo).FullName;
						case InfoType.DRIVE:
							return (this.InfoObject as DriveInfo).Name;
						default:
							return null;
					}
				}
			}
		}

		private void handle_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			UseWaitCursor = true;
			OnFileSystemFetchStarted(this, null);

			//Thread.Sleep(2000);
			TreeNode expandingNode = e.Node;
			TreeNodeTag tag = expandingNode.Tag as TreeNodeTag;
			switch (tag.Type)
			{
				default:
					break;
				case TreeNodeTag.InfoType.FOLDER:
					{
						DirectoryInfo expandingNodeInfo = tag.InfoObject as DirectoryInfo;
						RemoveLazyLoadingNode(expandingNode);
						if (expandingNode.Nodes.Count == 0)
							PopuplateDirectory(expandingNode, expandingNodeInfo);
						break;
					}
				case TreeNodeTag.InfoType.DRIVE:
					{
						DriveInfo expandingNodeInfo = tag.InfoObject as DriveInfo;
						RemoveLazyLoadingNode(expandingNode);
						if (expandingNode.Nodes.Count == 0)
							PopulateDrive(expandingNode, expandingNodeInfo);
						break;
					}
			}

			OnFileSystemFetchEnded(this, null);
			UseWaitCursor = false;
		}

		private void BuildTagDataList(TreeNode node, List<TreeNodeTag> list)
		{
			TreeNodeTag nodeTag = node.Tag as TreeNodeTag;
			// Skip over loading nodes and nodes without a tag.
			if (nodeTag == null || nodeTag.Type == TreeNodeTag.InfoType.LOADING)
				return;

			CheckState state = GetCheckState(node);
			switch (state)
			{
				case CheckState.Unchecked:
					// If it's unchecked, ignore it and its child nodes.
					return;
				case CheckState.Checked:
					// If it's checked, add it and ignore its child nodes.
					// This means the entire folder is checked - regardless of what it contains.
					list.Add(node.Tag as TreeNodeTag);
					break;
				case CheckState.Mixed:
					// Ignore it, but verify its child nodes.
					foreach (TreeNode child in node.Nodes)
						BuildTagDataList(child, list);
					break;
			}
		}

		public List<TreeNodeTag> GetCheckedTagData()
		{
			List<TreeNodeTag> list = new List<TreeNodeTag>();
			foreach (TreeNode node in Nodes)
				BuildTagDataList(node, list);
			return list;
		}

		#region Custom events

		public delegate void FileSystemFetchStartedHandler(object sender, EventArgs e);
		public delegate void FileSystemFetchEndedHandler(object sender, EventArgs e);

		public event FileSystemFetchStartedHandler FileSystemFetchStarted;
		public event FileSystemFetchEndedHandler FileSystemFetchEnded;

		protected virtual void OnFileSystemFetchStarted(object sender, EventArgs e)
		{
			if (FileSystemFetchStarted != null)
				FileSystemFetchStarted(this, e);
		}

		protected virtual void OnFileSystemFetchEnded(object sender, EventArgs e)
		{
			if (FileSystemFetchEnded != null)
				FileSystemFetchEnded(this, e);
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

	internal static class Native
	{
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;

		public const UInt32 TV_FIRST = 0x1100;
		public const UInt32 TVSIL_STATE = 2;
		public const UInt32 TVM_SETIMAGELIST = TV_FIRST + 9;
		public const UInt32 TVM_SETITEM = TV_FIRST + 63;

		public const UInt32 TVIF_STATE = 0x0008;
		public const UInt32 TVIF_HANDLE = 0x0010;

		public const UInt32 TVIS_STATEIMAGEMASK = 0xF000;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct TVITEMEX
		{
			public UInt32 mask;
			public IntPtr hItem;
			public UInt32 state;
			public UInt32 stateMask;
			public IntPtr pszText;
			public int cchTextMax;
			public int iImage;
			public int iSelectedImage;
			public int cChildren;
			public IntPtr lParam;
			public int iIntegral;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, ref TVITEMEX lParam);
	}
}
