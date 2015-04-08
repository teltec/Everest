using System;
using System.Collections.Generic;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Controls
{
	public sealed class BackupPlanTreeNodeData : EntryTreeNodeData
	{
		public Models.BackupPlan Plan { get; set; }
		public IFileVersion Version { get; private set; } // Depends on `InfoObject`

		public BackupPlanTreeNodeData()
		{
		}

		public BackupPlanTreeNodeData(BackupPlan plan, EntryInfo infoObject)
		{
			Plan = plan;
			InfoObject = infoObject;
		}

		protected override void UpdateProperties()
		{
			switch (Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "type");
				case TypeEnum.FILE_VERSION:
					Name = InfoObject.Name;
					Path = InfoObject.Path;
					Version = InfoObject.Version;
					break;
				case TypeEnum.FILE:
				case TypeEnum.FOLDER:
				case TypeEnum.DRIVE:
					Name = InfoObject.Name;
					Path = InfoObject.Path;
					Version = null;
					break;
			}
		}
	}

	public static class BackupPlanTreeNodeDataExtensions
	{
		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `RestorePlanSourceEntry`.
		public static List<RestorePlanSourceEntry> ToRestorePlanSourceEntry(
			this Dictionary<string, BackupPlanTreeNodeData> dataDict, RestorePlan plan, RestorePlanSourceEntryRepository dao)
		{
			List<RestorePlanSourceEntry> sources = new List<RestorePlanSourceEntry>(dataDict.Count);
			foreach (var entry in dataDict)
			{
				BackupPlanTreeNodeData data = entry.Value;
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
