using System;
using System.Collections.Generic;
using Teltec.Backup.App.DAO;
using Teltec.Common.Forms;

namespace Teltec.Backup.App.Models
{
	public static class FileSystemTreeNodeTagExtensions
	{
		public static EntryType ToEntryType(this FileSystemTreeNodeTag tag)
		{
			switch (tag.Type)
			{
				default: throw new ArgumentException("Unhandled TreeNodeTag.InfoType", "tag.type");
				case FileSystemTreeNodeTag.InfoType.DRIVE:
					return EntryType.DRIVE;
				case FileSystemTreeNodeTag.InfoType.FOLDER:
					return EntryType.FOLDER;
				case FileSystemTreeNodeTag.InfoType.FILE:
					return EntryType.FILE;
			}
		}

		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `BackupPlanSourceEntry`.
		public static List<BackupPlanSourceEntry> ToBackupPlanSourceEntry(
			this Dictionary<string, FileSystemTreeNodeTag> tags, BackupPlan plan, BackupPlanSourceEntryRepository dao)
		{
			List<BackupPlanSourceEntry> sources = new List<BackupPlanSourceEntry>(tags.Count);
			foreach (var entry in tags)
			{
				Teltec.Common.Forms.FileSystemTreeNodeTag tag = entry.Value;
				BackupPlanSourceEntry source = null;
				if (tag.Id != null)
					source = dao.Get(tag.Id as long?);
				else
					source = new BackupPlanSourceEntry();
				source.BackupPlan = plan;
				source.Type = tag.ToEntryType();
				source.Path = tag.Path;
				sources.Add(source);
			}
			return sources;
		}

		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `RestorePlanSourceEntry`.
		public static List<RestorePlanSourceEntry> ToRestorePlanSourceEntry(
			this Dictionary<string, FileSystemTreeNodeTag> tags, RestorePlan plan, RestorePlanSourceEntryRepository dao)
		{
			List<RestorePlanSourceEntry> sources = new List<RestorePlanSourceEntry>(tags.Count);
			foreach (var entry in tags)
			{
				Teltec.Common.Forms.FileSystemTreeNodeTag tag = entry.Value;
				RestorePlanSourceEntry source = null;
				if (tag.Id != null)
					source = dao.Get(tag.Id as long?);
				else
					source = new RestorePlanSourceEntry();
				source.RestorePlan = plan;
				source.Type = tag.ToEntryType();
				source.Path = tag.Path;
				sources.Add(source);
			}
			return sources;
		}
	}
}
