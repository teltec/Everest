using System;
using System.Collections.Generic;
using Teltec.Backup.App.DAO;
using Teltec.Common.Forms;

namespace Teltec.Backup.App.Models
{
	public static class FileSystemTreeNodeDataExtensions
	{
		public static EntryType ToEntryType(this FileSystemTreeNodeData data)
		{
			switch (data.Type)
			{
				default: throw new ArgumentException("Unhandled FileSystemTreeNodeTag.Type", "tag.type");
				case FileSystemTreeNode.TypeEnum.DRIVE:
					return EntryType.DRIVE;
				case FileSystemTreeNode.TypeEnum.FOLDER:
					return EntryType.FOLDER;
				case FileSystemTreeNode.TypeEnum.FILE:
					return EntryType.FILE;
			}
		}

		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `BackupPlanSourceEntry`.
		public static List<BackupPlanSourceEntry> ToBackupPlanSourceEntry(
			this Dictionary<string, FileSystemTreeNodeData> dataDict, BackupPlan plan, BackupPlanSourceEntryRepository dao)
		{
			List<BackupPlanSourceEntry> sources = new List<BackupPlanSourceEntry>(dataDict.Count);
			foreach (var entry in dataDict)
			{
				Teltec.Common.Forms.FileSystemTreeNodeData data = entry.Value;
				BackupPlanSourceEntry source = null;
				if (data.Id != null)
					source = dao.Get(data.Id as long?);
				else
					source = new BackupPlanSourceEntry();
				source.BackupPlan = plan;
				source.Type = data.ToEntryType();
				source.Path = data.Path;
				sources.Add(source);
			}
			return sources;
		}

		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `RestorePlanSourceEntry`.
		public static List<RestorePlanSourceEntry> ToRestorePlanSourceEntry(
			this Dictionary<string, FileSystemTreeNodeData> dataDict, RestorePlan plan, RestorePlanSourceEntryRepository dao)
		{
			List<RestorePlanSourceEntry> sources = new List<RestorePlanSourceEntry>(dataDict.Count);
			foreach (var entry in dataDict)
			{
				Teltec.Common.Forms.FileSystemTreeNodeData data = entry.Value;
				RestorePlanSourceEntry source = null;
				if (data.Id != null)
					source = dao.Get(data.Id as long?);
				else
					source = new RestorePlanSourceEntry();
				source.RestorePlan = plan;
				source.Type = data.ToEntryType();
				source.Path = data.Path;
				sources.Add(source);
			}
			return sources;
		}
	}
}
