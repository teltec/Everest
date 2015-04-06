using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teltec.Common.Controls;
using Teltec.FileSystem;

namespace Teltec.Backup.App.Controls
{
	public sealed class FileSystemTreeView : AdvancedTreeView
	{
		public FileSystemTreeView()
			: base()
		{
			this.UseWaitCursor = true;
			PopulateTreeView();
			this.UseWaitCursor = false;

			this.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.handle_BeforeExpand);
			this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.handle_AfterCheck);
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
			PopulateDrives();
			ResumeLayout(false);
		}

		public void PopulateDrives()
		{
			try
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				foreach (var drive in drives)
				{
					string infoName = null;
					try
					{
						string driveLabel = drive.VolumeLabel;
						if (string.IsNullOrEmpty(driveLabel))
							infoName = drive.Name;
						else
							infoName = string.Format("{0} ({1})", drive.Name, driveLabel);
					}
					catch (Exception)
					{
						infoName = drive.Name;
					}

					EntryInfo info = new EntryInfo(TypeEnum.DRIVE, infoName, drive.Name);
					FileSystemTreeNode driveNode = FileSystemTreeNode.CreateDriveNode(info);
					this.Nodes.Add(driveNode);
					RestoreNodeState(driveNode);
				}
			}
			catch (System.SystemException e)
			{
				ShowErrorMessage(e, null);
			}
		}

		#endregion

		#region Custom events

		public delegate void ExpandFetchStartedHandler(object sender, EventArgs e);
		public delegate void ExpandFetchEndedHandler(object sender, EventArgs e);

		public event ExpandFetchStartedHandler ExpandFetchStarted;
		public event ExpandFetchEndedHandler ExpandFetchEnded;

		private void OnExpandFetchStarted(object sender, EventArgs e)
		{
			if (ExpandFetchStarted != null)
				ExpandFetchStarted(this, e);
		}

		private void OnExpandFetchEnded(object sender, EventArgs e)
		{
			if (ExpandFetchEnded != null)
				ExpandFetchEnded(this, e);
		}

		#endregion

		private void handle_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			UseWaitCursor = true;
			OnExpandFetchStarted(this, null);

			EntryTreeNode expandingNode = e.Node as EntryTreeNode;

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
			RestoreNodeStateRecursively(expandingNode as FileSystemTreeNode);

			// Then resume the layout to let it apply all changes made to the UI.
			ResumeLayout();

			OnExpandFetchEnded(this, null);
			UseWaitCursor = false;
		}

		private void handle_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			RestoreNodeStateRemove(e.Node as FileSystemTreeNode);
		}

		#region CheckState saving

		private void BuildTagDataDict(FileSystemTreeNode node, Dictionary<string, FileSystemTreeNodeData> dict)
		{
			// Skip over loading nodes and nodes without a tag.
			if (node == null || node.Data.Type == TypeEnum.LOADING)
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
				EntryTreeNodeData newTag = null;
				switch (nodeParent.Type)
				{
					case PathNode.TypeEnum.FILE:
						{
							EntryInfo info = new EntryInfo(TypeEnum.FILE, nodeParent.Name, nodeParent.Path);
							newTag = new FileSystemTreeNodeData(info);
							newTag.State = CheckState.Mixed;
							break;
						}
					case PathNode.TypeEnum.FOLDER:
						{
							EntryInfo info = new EntryInfo(TypeEnum.FOLDER, nodeParent.Name, nodeParent.Path);
							newTag = new FileSystemTreeNodeData(info);
							newTag.State = CheckState.Mixed;
							break;
						}
					case PathNode.TypeEnum.DRIVE:
						{
							EntryInfo info = new EntryInfo(TypeEnum.DRIVE, nodeParent.Name, nodeParent.Path);
							newTag = new FileSystemTreeNodeData(info);
							newTag.State = CheckState.Mixed;
							break;
						}
				}

				if (newTag != null)
				{
					if (!expandedDict.ContainsKey(nodeParent.Path))
						expandedDict.Add(nodeParent.Path, newTag as FileSystemTreeNodeData);
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

			// Expand paths into their respective parts.
			foreach (var obj in dict)
			{
				switch (obj.Value.Type)
				{
					default: throw new ArgumentException("Unhandled TypeEnum", "TreeNodeData.Type");
					case TypeEnum.FILE:
					case TypeEnum.FOLDER:
						hasParents = true;
						break;
					case TypeEnum.DRIVE:
						hasParents = false;
						break;
				}
				if (obj.Value.InfoObject == null)
					obj.Value.InfoObject = new EntryInfo(obj.Value.Type, obj.Value.Name, obj.Value.Path);
				expandedDict.Add(obj.Key, obj.Value);
				if (hasParents)
					ExpandCheckedDataSourceAddParents(expandedDict, obj.Value.Path);
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
				case TypeEnum.LOADING:
					// Ignore this node.
					break;
				case TypeEnum.DRIVE:
				case TypeEnum.FOLDER:
				case TypeEnum.FILE:
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

			if (node.Data.Type != TypeEnum.LOADING)
			{
				string path = node.Data.Path;
				if (CheckedDataSource.ContainsKey(path))
					CheckedDataSource.Remove(path);
			}
		}

		#endregion
	}
}
