using System;
using System.Collections.Generic;
using System.IO;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;

namespace Teltec.Backup.App.Controls
{
	public sealed class FileSystemTreeNodeData : EntryTreeNodeData
	{
		public FileSystemTreeNodeData()
		{
		}

		public FileSystemTreeNodeData(EntryInfo infoObject)
		{
			InfoObject = infoObject;
		}

		protected override void UpdateProperties()
		{
			switch (Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "type");
				case TypeEnum.FILE:
				case TypeEnum.FOLDER:
				case TypeEnum.DRIVE:
					Path = InfoObject.Path;
					break;
			}
		}
	}

	public static class FileSystemTreeNodeDataExtensions
	{
		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `BackupPlanSourceEntry`.
		public static List<BackupPlanSourceEntry> ToBackupPlanSourceEntry(
			this Dictionary<string, FileSystemTreeNodeData> dataDict, BackupPlan plan, BackupPlanSourceEntryRepository dao)
		{
			List<BackupPlanSourceEntry> sources = new List<BackupPlanSourceEntry>(dataDict.Count);
			foreach (var entry in dataDict)
			{
				FileSystemTreeNodeData data = entry.Value;
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
	}
}
