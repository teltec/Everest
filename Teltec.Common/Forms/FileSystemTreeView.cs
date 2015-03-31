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
					FileSystemTreeNode driveNode = FileSystemTreeNode.CreateDriveNode(drive);
					this.Nodes.Add(driveNode);
					RestoreNodeState(driveNode);
				}
			}
			catch (System.SystemException e)
			{
				ShowErrorMessage(e, null);
			}

			ResumeLayout(false);
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

			FileSystemTreeNode expandingNode = e.Node as FileSystemTreeNode;

			// Suspend layout because the expanding node may change the UI.
			SuspendLayout();

			try
			{
				//Thread.Sleep(2000);
				// Signal the node to do something during the expanding.
				// We wouldn't need this, but we want to handle exceptions and such.
				expandingNode.OnExpand();
			}
			catch (System.SystemException ex)
			{
				ShowErrorMessage(ex, expandingNode);
			}

			// Restore nodes state.
			RestoreNodeStateRecursively(expandingNode);

			// Then resume the layout to let it apply all changes made to the UI.
			ResumeLayout();

			OnFileSystemFetchEnded(this, null);
			UseWaitCursor = false;
		}

		private void handle_AfterCheck(object sender, TreeViewEventArgs e)
		{
			RestoreNodeStateRemove(e.Node as FileSystemTreeNode);
		}

		#region CheckState saving

		private void BuildTagDataDict(FileSystemTreeNode node, Dictionary<string, FileSystemTreeNodeData> dict)
		{
			// Skip over loading nodes and nodes without a tag.
			if (node == null || node.Data.Type == FileSystemTreeNode.TypeEnum.LOADING)
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
						string path = node.Data.Path;
						FileSystemTreeNodeData match;
						bool found = dict.TryGetValue(path, out match);
						match = found ? match : node.Data;
						match.State = CheckState.Checked;
						node.Data = match;
						if (!dict.ContainsKey(match.Path))
							dict.Add(match.Path, match);
					}
					else
					{
						FileSystemTreeNodeData tag = node.Data;
						tag.State = CheckState.Checked;
						if (!dict.ContainsKey(tag.Path))
							dict.Add(tag.Path, tag);
					}
					break;
				case CheckState.Mixed:
					// Ignore it, but verify its child nodes.
					foreach (FileSystemTreeNode child in node.Nodes)
						BuildTagDataDict(child, dict);
					break;
			}
		}

		public Dictionary<string, FileSystemTreeNodeData> GetCheckedTagData()
		{
			// ...
			Dictionary<string, FileSystemTreeNodeData> dict = CheckedDataSource != null
				? CheckedDataSource.Select(e => e.Value).Where(e => e.State == CheckState.Checked)
					.ToDictionary(k => k.Path)
				: new Dictionary<string, FileSystemTreeNodeData>();
			// ...
			foreach (FileSystemTreeNode node in Nodes)
			{
				if (CheckedDataSource != null)
				{
					string path = node.Data.Path;
					FileSystemTreeNodeData match;
					bool found = dict.TryGetValue(path, out match);
					if (found)
						node.Data = match;
					BuildTagDataDict(node as FileSystemTreeNode, dict);
				}
				else
				{
					BuildTagDataDict(node as FileSystemTreeNode, dict);
				}
			}
			return dict;
		}

		#endregion

		#region CheckState restoring

		private Dictionary<string, FileSystemTreeNodeData> _CheckedDataSource;
		public Dictionary<string, FileSystemTreeNodeData> CheckedDataSource
		{
			get { return _CheckedDataSource; }
			set { _CheckedDataSource = ExpandCheckedDataSource(value); PopulateTreeView(); }
		}

		private void ExpandCheckedDataSourceAddParents(Dictionary<string, FileSystemTreeNodeData> expandedDict, string path)
		{
			PathNodes nodes = new PathNodes(path);
			PathNode nodeParent = nodes.Parent;

			while (nodeParent != null)
			{
				FileSystemTreeNodeData newTag = null;
				switch (nodeParent.Type)
				{
					case PathNode.TypeEnum.FOLDER:
						newTag = new FileSystemTreeNodeData
						{
							Type = FileSystemTreeNode.TypeEnum.FOLDER,
							InfoObject = new DirectoryInfo(nodeParent.Path),
							State = CheckState.Mixed
						};
						break;
					case PathNode.TypeEnum.DRIVE:
						newTag = new FileSystemTreeNodeData
						{
							Type = FileSystemTreeNode.TypeEnum.DRIVE,
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

		private Dictionary<string, FileSystemTreeNodeData> ExpandCheckedDataSource(
			Dictionary<string, FileSystemTreeNodeData> dict)
		{
			if (dict == null)
				return null;

			Dictionary<string, FileSystemTreeNodeData> expandedDict =
				new Dictionary<string, FileSystemTreeNodeData>(dict.Count * 2);

			bool hasParents = false;
			string path = null;

			// Expand paths into their respective parts.
			foreach (var obj in dict)
			{
				path = obj.Value.Path;
				switch (obj.Value.Type)
				{
					case FileSystemTreeNode.TypeEnum.FILE:
						{
							hasParents = true;
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new FileInfo(path);
							break;
						}
					case FileSystemTreeNode.TypeEnum.FOLDER:
						{
							hasParents = true;
							if (obj.Value.InfoObject == null)
								obj.Value.InfoObject = new DirectoryInfo(path);
							break;
						}
					case FileSystemTreeNode.TypeEnum.DRIVE:
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

		private void RestoreNodeStateRecursively(FileSystemTreeNode node)
		{
			//if (GetCheckState(parentNode) == CheckState.Checked)
			//{
			//	//SetCheckState(node, CheckState.Checked);
			//}
			//else
			//{
				RestoreNodeState(node);
			//}

			foreach (FileSystemTreeNode subNode in node.Nodes)
				RestoreNodeStateRecursively(subNode);
		}

		private void RestoreNodeState(FileSystemTreeNode node)
		{
			if (CheckedDataSource == null)
				return;

			switch (node.Data.Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "type");
				case FileSystemTreeNode.TypeEnum.LOADING:
					// Ignore this node.
					break;
				case FileSystemTreeNode.TypeEnum.DRIVE:
				case FileSystemTreeNode.TypeEnum.FOLDER:
				case FileSystemTreeNode.TypeEnum.FILE:
					{
						string path = node.Data.Path;
						FileSystemTreeNodeData match;
						bool found = CheckedDataSource.TryGetValue(path, out match);
						if (found && match.Type == node.Data.Type)
						{
							node.Data = match;
							SetStateImage(node, (int)match.State);
						}
						break;
					}
			}
		}

		private void RestoreNodeStateRemove(FileSystemTreeNode node)
		{
			if (CheckedDataSource == null)
				return;
			if (node.Data == null)
				return;

			if (node.Data.Type != FileSystemTreeNode.TypeEnum.LOADING)
			{
				string path = node.Data.Path;
				if (CheckedDataSource.ContainsKey(path))
					CheckedDataSource.Remove(path);
			}
		}

		#endregion
	}
}
