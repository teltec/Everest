using System;
using System.Collections.Generic;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Data.FileSystem;
using Teltec.Storage.Versioning;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Controls
{
	public sealed class BackupPlanTreeNodeData : EntryTreeNodeData
	{
		public Models.StorageAccount StorageAccount { get; set; }
		public IFileVersion Version { get; private set; } // Depends on `InfoObject`
		public object UserObject { get; set; }

		public BackupPlanTreeNodeData()
		{
		}

		public BackupPlanTreeNodeData(Models.StorageAccount account, EntryInfo infoObject)
		{
			StorageAccount = account;
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
		public static List<Models.RestorePlanSourceEntry> ToRestorePlanSourceEntry(
			this Dictionary<string, BackupPlanTreeNodeData> dataDict, Models.RestorePlan plan, RestorePlanSourceEntryRepository dao)
		{
			List<Models.RestorePlanSourceEntry> sources = new List<Models.RestorePlanSourceEntry>(dataDict.Count);
			foreach (var entry in dataDict)
			{
				BackupPlanTreeNodeData data = entry.Value;
				Models.RestorePlanSourceEntry source = null;
				if (data.Id != null)
					source = dao.Get(data.Id as long?);
				else
					source = new Models.RestorePlanSourceEntry();
				source.RestorePlan = plan;
				source.Type = data.ToEntryType();
				source.Path = data.Path;
				source.PathNode = data.UserObject as Models.BackupPlanPathNode;
				if (source.Type == Models.EntryType.FILE_VERSION)
					source.Version = data.Version.Version;
				sources.Add(source);
			}
			return sources;
		}
	}
}
