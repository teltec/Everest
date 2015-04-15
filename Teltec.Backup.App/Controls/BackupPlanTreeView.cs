using System;
using System.Collections.Generic;
using System.Linq;
using Teltec.Backup.App.DAO;
using Teltec.Common.Controls;
using Teltec.FileSystem;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Controls
{
	public sealed class BackupPlanTreeView : AdvancedTreeView
	{
		public BackupPlanTreeView()
			: base()
		{
			this.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.handle_BeforeExpand);
			this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.handle_AfterCheck);
		}

		private Models.BackupPlan _Plan;
		public Models.BackupPlan Plan
		{
			get { return _Plan; }
			set { _Plan = value; OnPlanChanged(); }
		}

		private void OnPlanChanged()
		{
			this.UseWaitCursor = true;
			PopulateTreeView();
			this.UseWaitCursor = false;
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

		private IFileVersion BuildVersion(Models.BackupPlan plan)
		{
			if (!plan.Id.HasValue)
				return null;
			IFileVersion obj = new FileVersion();
			obj.Version = plan.Id.Value.ToString();
			return obj;
		}

		public void PopulateDrives()
		{
			if (Plan == null)
				return;

			BackupPlanPathNodeRepository dao = new BackupPlanPathNodeRepository();

			try
			{
				//IFileVersion version = BuildVersion(Plan);
				IList<Models.BackupPlanPathNode> drives = dao.GetAllDrivesByPlan(Plan);

				foreach (var drive in drives)
				{
					//EntryInfo info = new EntryInfo(TypeEnum.DRIVE, drive.Name, drive.Name, version);
					BackupPlanTreeNode driveNode = BackupPlanTreeNode.CreateDriveNode(drive);
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
			RestoreNodeStateRecursively(expandingNode as BackupPlanTreeNode);

			// Then resume the layout to let it apply all changes made to the UI.
			ResumeLayout();

			OnExpandFetchEnded(this, null);
			UseWaitCursor = false;
		}

		private void handle_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			RestoreNodeStateRemove(e.Node as BackupPlanTreeNode);
		}

		#region CheckState saving

		private void BuildTagDataDict(BackupPlanTreeNode node, Dictionary<string, BackupPlanTreeNodeData> dict)
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
						BackupPlanTreeNodeData match;
						bool found = dict.TryGetValue(path, out match);
						match = found ? match : node.Data;
						match.State = CheckState.Checked;
						node.Data = match;
						if (!dict.ContainsKey(match.Path))
							dict.Add(match.Path, match);
					}
					else
					{
						BackupPlanTreeNodeData tag = node.Data;
						tag.State = CheckState.Checked;
						if (!dict.ContainsKey(tag.Path))
							dict.Add(tag.Path, tag);
					}
					break;
				case CheckState.Mixed:
					// Ignore it, but verify its child nodes.
					foreach (BackupPlanTreeNode child in node.Nodes)
						BuildTagDataDict(child, dict);
					break;
			}
		}

		public Dictionary<string, BackupPlanTreeNodeData> GetCheckedTagData()
		{
			// ...
			Dictionary<string, BackupPlanTreeNodeData> dict = CheckedDataSource != null
				? CheckedDataSource.Select(e => e.Value).Where(e => e.State == CheckState.Checked)
					.ToDictionary(k => k.Path)
				: new Dictionary<string, BackupPlanTreeNodeData>();
			// ...
			foreach (BackupPlanTreeNode node in Nodes)
			{
				if (CheckedDataSource != null)
				{
					string path = node.Data.Path;
					BackupPlanTreeNodeData match;
					bool found = dict.TryGetValue(path, out match);
					if (found)
						node.Data = match;
					BuildTagDataDict(node as BackupPlanTreeNode, dict);
				}
				else
				{
					BuildTagDataDict(node as BackupPlanTreeNode, dict);
				}
			}
			return dict;
		}

		#endregion

		#region CheckState restoring

		private Dictionary<string, BackupPlanTreeNodeData> _CheckedDataSource;
		public Dictionary<string, BackupPlanTreeNodeData> CheckedDataSource
		{
			get { return _CheckedDataSource; }
			set { _CheckedDataSource = ExpandCheckedDataSource(value); PopulateTreeView(); }
		}

		private void ExpandCheckedDataSourceAddParents(Dictionary<string, BackupPlanTreeNodeData> expandedDict, string path)
		{
			IFileVersion version = BuildVersion(Plan);
			PathNodes nodes = new PathNodes(path);
			PathNode nodeParent = nodes.ParentNode;

			while (nodeParent != null)
			{
				EntryTreeNodeData newTag = null;
				switch (nodeParent.Type)
				{
					case PathNode.TypeEnum.FILE:
						{
							EntryInfo info = new EntryInfo(TypeEnum.FILE, nodeParent.Name, nodeParent.Path, version);
							newTag = new BackupPlanTreeNodeData(Plan, info);
							newTag.State = CheckState.Mixed;
							break;
						}
					case PathNode.TypeEnum.FOLDER:
						{
							EntryInfo info = new EntryInfo(TypeEnum.FOLDER, nodeParent.Name, nodeParent.Path, version);
							newTag = new BackupPlanTreeNodeData(Plan, info);
							newTag.State = CheckState.Mixed;
							break;
						}
					case PathNode.TypeEnum.DRIVE:
						{
							EntryInfo info = new EntryInfo(TypeEnum.DRIVE, nodeParent.Name, nodeParent.Path, version);
							newTag = new BackupPlanTreeNodeData(Plan, info);
							newTag.State = CheckState.Mixed;
							break;
						}
				}

				if (newTag != null)
				{
					if (!expandedDict.ContainsKey(nodeParent.Path))
						expandedDict.Add(nodeParent.Path, newTag as BackupPlanTreeNodeData);
				}

				nodeParent = nodeParent.Parent;
			}
		}

		private Dictionary<string, BackupPlanTreeNodeData> ExpandCheckedDataSource(
			Dictionary<string, BackupPlanTreeNodeData> dict)
		{
			if (dict == null)
				return null;

			IFileVersion version = BuildVersion(Plan);

			Dictionary<string, BackupPlanTreeNodeData> expandedDict =
				new Dictionary<string, BackupPlanTreeNodeData>(dict.Count * 2);

			bool hasParents = false;

			// Expand paths into their respective parts.
			foreach (var obj in dict)
			{
				switch (obj.Value.Type)
				{
					default: throw new ArgumentException("Unhandled TypeEnum", "obj.Value.Type");
					case TypeEnum.FILE_VERSION:
					case TypeEnum.FILE:
					case TypeEnum.FOLDER:
						hasParents = true;
						break;
					case TypeEnum.DRIVE:
						hasParents = false;
						break;
				}
				if (obj.Value.InfoObject == null)
					obj.Value.InfoObject = new EntryInfo(obj.Value.Type, obj.Value.Name, obj.Value.Path, version);
				expandedDict.Add(obj.Key, obj.Value);
				if (hasParents)
					ExpandCheckedDataSourceAddParents(expandedDict, obj.Value.Path);
			}

			return expandedDict;
		}

		private void RestoreNodeStateRecursively(BackupPlanTreeNode node)
		{
			//if (GetCheckState(parentNode) == CheckState.Checked)
			//{
			//	//SetCheckState(node, CheckState.Checked);
			//}
			//else
			//{
				RestoreNodeState(node);
			//}

			foreach (BackupPlanTreeNode subNode in node.Nodes)
				RestoreNodeStateRecursively(subNode);
		}

		private void RestoreNodeState(BackupPlanTreeNode node)
		{
			if (CheckedDataSource == null)
				return;

			switch (node.Data.Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "node.Data.Type");
				case TypeEnum.LOADING:
					// Ignore this node.
					break;
				case TypeEnum.DRIVE:
				case TypeEnum.FOLDER:
				case TypeEnum.FILE:
				case TypeEnum.FILE_VERSION:
					{
						string path = node.Data.Path;
						BackupPlanTreeNodeData match;
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

		private void RestoreNodeStateRemove(BackupPlanTreeNode node)
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
