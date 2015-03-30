using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teltec.FileSystem;

namespace Teltec.Common.Forms
{
	public class FileSystemTreeView : AdvancedTreeView
	{
		public FileSystemTreeView()
			: base()
		{
			this.UseWaitCursor = true;
			PopulateTreeView();
			this.UseWaitCursor = false;

			this.BeforeExpand += new TreeViewCancelEventHandler(this.handle_BeforeExpand);
			this.AfterCheck += new TreeViewEventHandler(this.handle_AfterCheck);
		}

		#region Populate methods

		public void PopulateTreeView()
		{
			if (DesignMode)
				return;

			if (!IsHandleCreated)
				CreateHandle();

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
			//if (GetCheckState(parentNode) == CheckState.Checked)
			//{
			//	//SetCheckState(node, CheckState.Checked);
			//}
			//else
			//{
				RestoreNodeState(FileSystemTreeNodeTag.InfoType.FOLDER, node);
			//}
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
			//if (GetCheckState(parentNode) == CheckState.Checked)
			//{
			//	//SetCheckState(node, CheckState.Checked);
			//}
			//else
			//{
				RestoreNodeState(FileSystemTreeNodeTag.InfoType.FILE, node);
			//}
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

		private void handle_AfterCheck(object sender, TreeViewEventArgs e)
		{
			RestoreNodeStateRemove(e.Node);
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
						if (!dict.ContainsKey(match.Path))
							dict.Add(match.Path, match);
					}
					else
					{
						FileSystemTreeNodeTag tag = node.Tag as FileSystemTreeNodeTag;
						tag.State = CheckState.Checked;
						if (!dict.ContainsKey(tag.Path))
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

		private void ExpandCheckedDataSourceAddParents(Dictionary<string, FileSystemTreeNodeTag> expandedDict, string path)
		{
			PathNodes nodes = new PathNodes(path);
			PathNode nodeParent = nodes.Parent;

			while (nodeParent != null)
			{
				FileSystemTreeNodeTag newTag = null;
				switch (nodeParent.Type)
				{
					case PathNode.TypeEnum.FOLDER:
						newTag = new FileSystemTreeNodeTag
						{
							Type = FileSystemTreeNodeTag.InfoType.FOLDER,
							InfoObject = new DirectoryInfo(nodeParent.Path),
							State = CheckState.Mixed
						};
						break;
					case PathNode.TypeEnum.DRIVE:
						newTag = new FileSystemTreeNodeTag
						{
							Type = FileSystemTreeNodeTag.InfoType.DRIVE,
							InfoObject = new DriveInfo(nodeParent.Path),
							State = CheckState.Mixed
						};
						break;
				}

				if (newTag != null)
				{
					if (!expandedDict.ContainsKey(nodeParent.Path))
						expandedDict.Add(nodeParent.Path, newTag);
				}

				nodeParent = nodeParent.Parent;
			}
		}

		private Dictionary<string, FileSystemTreeNodeTag> ExpandCheckedDataSource(
			Dictionary<string, FileSystemTreeNodeTag> dict)
		{
			if (dict == null)
				return null;

			Dictionary<string, FileSystemTreeNodeTag> expandedDict =
				new Dictionary<string, FileSystemTreeNodeTag>(dict.Count * 2);

			bool hasParents = false;
			string path = null;

			// Expand paths into their respective parts.
			foreach (var obj in dict)
			{
				path = obj.Value.Path;
				switch (obj.Value.Type)
				{
					case FileSystemTreeNodeTag.InfoType.FILE:
						{
							hasParents = true;
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new FileInfo(path);
							break;
						}
					case FileSystemTreeNodeTag.InfoType.FOLDER:
						{
							hasParents = true;
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new DirectoryInfo(path);
							break;
						}
					case FileSystemTreeNodeTag.InfoType.DRIVE:
						{
							hasParents = false;
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new DriveInfo(path);
							break;
						}
				}
				expandedDict.Add(obj.Key, obj.Value);
				if (hasParents)
					ExpandCheckedDataSourceAddParents(expandedDict, path);
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

		private void RestoreNodeStateRemove(TreeNode node)
		{
			if (CheckedDataSource == null)
				return;
			if (node.Tag == null)
				return;

			FileSystemTreeNodeTag tag = node.Tag as FileSystemTreeNodeTag;
			if (tag.Type != FileSystemTreeNodeTag.InfoType.LOADING)
			{
				string path = FileSystemTreeNodeTag.BuildPath(tag);
				if (CheckedDataSource.ContainsKey(path))
					CheckedDataSource.Remove(path);
			}
		}

		#endregion
	}
}
