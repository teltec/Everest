using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
			CreateStateImages();
			PopulateTreeView();

			UseWaitCursor = false;

			this.BeforeExpand += new TreeViewCancelEventHandler(this.handle_BeforeExpand);
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

		internal void SetStateImage(TreeNode node, int index)
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
				case NativeMethods.WM_LBUTTONDBLCLK:
					{
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

		#region Populate methods

		public void PopulateTreeView()
		{
			if (DesignMode)
				return;

			SuspendLayout();
			this.Nodes.Clear();
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
			ResumeLayout(false);
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
			node.Tag = new FileSystemTreeNodeTag
			{
				Type = FileSystemTreeNodeTag.InfoType.DRIVE,
				InfoObject = drive
			};
			node.ImageKey = "drive";
			view.Nodes.Add(node);
			RestoreNodeState(FileSystemTreeNodeTag.InfoType.DRIVE, node);
			return node;
		}

		private TreeNode AddFolderNode(TreeNode parentNode, DirectoryInfo folder)
		{
			TreeNode node = new TreeNode(folder.Name, 0, 0);
			node.Tag = new FileSystemTreeNodeTag
			{
				Type = FileSystemTreeNodeTag.InfoType.FOLDER,
				InfoObject = folder
			};
			node.ImageKey = "folder";
			parentNode.Nodes.Add(node);
			if (GetCheckState(parentNode) == CheckState.Checked)
				SetCheckState(node, CheckState.Checked);
			else
				RestoreNodeState(FileSystemTreeNodeTag.InfoType.FOLDER, node);
			return node;
		}

		private TreeNode AddFileNode(TreeNode parentNode, FileInfo file)
		{
			TreeNode node = new TreeNode(file.Name, 0, 0);
			node.Tag = new FileSystemTreeNodeTag
			{
				Type = FileSystemTreeNodeTag.InfoType.FILE,
				InfoObject = file
			};
			node.ImageKey = "file";
			parentNode.Nodes.Add(node);
			if (GetCheckState(parentNode) == CheckState.Checked)
				SetCheckState(node, CheckState.Checked);
			else
				RestoreNodeState(FileSystemTreeNodeTag.InfoType.FILE, node);
			return node;
		}

		private TreeNode AddLazyLoadingNode(TreeNode parentNode)
		{
			TreeNode node = new TreeNode("Retrieving data...", 0, 0);
			node.Tag = new FileSystemTreeNodeTag { Type = FileSystemTreeNodeTag.InfoType.LOADING };
			node.ImageKey = "loading";
			parentNode.Nodes.Add(node);
			return node;
		}

		private void RemoveLazyLoadingNode(TreeNode parentNode)
		{
			TreeNode firstChildNode = parentNode.FirstNode;
			if (firstChildNode == null)
				return;

			FileSystemTreeNodeTag tag = firstChildNode.Tag as FileSystemTreeNodeTag;
			if (tag != null && tag.Type == FileSystemTreeNodeTag.InfoType.LOADING)
				firstChildNode.Remove();
		}

		#endregion

		private void handle_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			UseWaitCursor = true;
			OnFileSystemFetchStarted(this, null);

			//Thread.Sleep(2000);
			TreeNode expandingNode = e.Node;
			FileSystemTreeNodeTag tag = expandingNode.Tag as FileSystemTreeNodeTag;
			switch (tag.Type)
			{
				default:
					break;
				case FileSystemTreeNodeTag.InfoType.FOLDER:
					{
						DirectoryInfo expandingNodeInfo = tag.InfoObject as DirectoryInfo;
						RemoveLazyLoadingNode(expandingNode);
						if (expandingNode.Nodes.Count == 0)
							PopuplateDirectory(expandingNode, expandingNodeInfo);
						break;
					}
				case FileSystemTreeNodeTag.InfoType.DRIVE:
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

		private void BuildTagDataDict(TreeNode node, Dictionary<string, FileSystemTreeNodeTag> dict)
		{
			FileSystemTreeNodeTag nodeTag = node.Tag as FileSystemTreeNodeTag;
			// Skip over loading nodes and nodes without a tag.
			if (nodeTag == null || nodeTag.Type == FileSystemTreeNodeTag.InfoType.LOADING)
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
					if (CheckedDataSource != null)
					{
						string path = FileSystemTreeNodeTag.BuildPath(node.Tag as FileSystemTreeNodeTag);
						FileSystemTreeNodeTag match;
						bool found = dict.TryGetValue(path, out match);
						match = found ? match : node.Tag as FileSystemTreeNodeTag;
						match.State = CheckState.Checked;
						node.Tag = match;
						dict.Add(match.Path, match);
					}
					else
					{
						FileSystemTreeNodeTag tag = node.Tag as FileSystemTreeNodeTag;
						tag.State = CheckState.Checked;
						dict.Add(tag.Path, tag);
					}
					break;
				case CheckState.Mixed:
					// Ignore it, but verify its child nodes.
					foreach (TreeNode child in node.Nodes)
						BuildTagDataDict(child, dict);
					break;
			}
		}

		public Dictionary<string, FileSystemTreeNodeTag> GetCheckedTagData()
		{
			// ...
			Dictionary<string, FileSystemTreeNodeTag> dict = CheckedDataSource != null
				? CheckedDataSource.Select(e => e.Value).Where(e => e.State == CheckState.Checked)
					.ToDictionary(k => k.Path)
				: new Dictionary<string, FileSystemTreeNodeTag>();
			// ...
			foreach (TreeNode node in Nodes)
			{
				if (CheckedDataSource != null)
				{
					string path = FileSystemTreeNodeTag.BuildPath(node.Tag as FileSystemTreeNodeTag);
					FileSystemTreeNodeTag match;
					bool found = dict.TryGetValue(path, out match);
					if (found)
						node.Tag = match;
					BuildTagDataDict(node, dict);
				}
				else
				{
					BuildTagDataDict(node, dict);
				}
			}
			return dict;
		}

		#region CheckState restoring

		private Dictionary<string, FileSystemTreeNodeTag> _CheckedDataSource;
		public Dictionary<string, FileSystemTreeNodeTag> CheckedDataSource
		{
			get { return _CheckedDataSource; }
			set { _CheckedDataSource = ExpandCheckedDataSource(value); PopulateTreeView(); }
		}

		private Dictionary<string, FileSystemTreeNodeTag> ExpandCheckedDataSource(
			Dictionary<string, FileSystemTreeNodeTag> dict)
		{
			Dictionary<string, FileSystemTreeNodeTag> expandedDict =
				new Dictionary<string, FileSystemTreeNodeTag>(dict.Count * 2);

			// Expand paths into their respective parts.
			foreach (var obj in dict)
			{
				switch (obj.Value.Type)
				{
					case FileSystemTreeNodeTag.InfoType.DRIVE:
						{
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new DriveInfo(obj.Value.Path);
							expandedDict.Add(obj.Key, obj.Value);
							break;
						}
					case FileSystemTreeNodeTag.InfoType.FILE:
						{
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new FileInfo(obj.Value.Path);
							expandedDict.Add(obj.Key, obj.Value);
							break;
						}
					case FileSystemTreeNodeTag.InfoType.FOLDER:
						{
							DirectoryInfo info = new DirectoryInfo(obj.Value.Path);
							DirectoryInfo parentInfo = info.Parent;
							while (parentInfo != null)
							{
								FileSystemTreeNodeTag.InfoType type = parentInfo.Parent != null
									? FileSystemTreeNodeTag.InfoType.FOLDER
									: FileSystemTreeNodeTag.InfoType.DRIVE;
								switch (type)
								{
									case FileSystemTreeNodeTag.InfoType.FOLDER:
										{
											FileSystemTreeNodeTag newTag = new FileSystemTreeNodeTag
											{
												Type = type,
												InfoObject = parentInfo,
												State = CheckState.Mixed
											};
											if (!expandedDict.ContainsKey(newTag.Path))
												expandedDict.Add(newTag.Path, newTag);
											break;
										}
									case FileSystemTreeNodeTag.InfoType.DRIVE:
										{
											FileSystemTreeNodeTag newTag = new FileSystemTreeNodeTag
											{
												Type = type,
												InfoObject = new DriveInfo(parentInfo.Name),
												State = CheckState.Mixed
											};
											if (!expandedDict.ContainsKey(newTag.Path))
												expandedDict.Add(newTag.Path, newTag);
											break;
										}
								}
								parentInfo = parentInfo.Parent;
							}

							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new DirectoryInfo(obj.Value.Path);
							expandedDict.Add(obj.Key, obj.Value);
							break;
						}
				}
			}

			return expandedDict;
		}

		private void RestoreNodeState(FileSystemTreeNodeTag.InfoType type, TreeNode node)
		{
			if (CheckedDataSource == null)
				return;

			switch (type)
			{
				default:
					throw new ArgumentException("Unhandled InfoType", "type");
				case FileSystemTreeNodeTag.InfoType.DRIVE:
				case FileSystemTreeNodeTag.InfoType.FOLDER:
				case FileSystemTreeNodeTag.InfoType.FILE:
					{
						string path = FileSystemTreeNodeTag.BuildPath(node.Tag as FileSystemTreeNodeTag);
						FileSystemTreeNodeTag match;
						bool found = CheckedDataSource.TryGetValue(path, out match);
						if (found && match.Type == type)
						{
							node.Tag = match;
							SetStateImage(node, (int)match.State);
						}
						break;
					}
			}
		}

		#endregion

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
