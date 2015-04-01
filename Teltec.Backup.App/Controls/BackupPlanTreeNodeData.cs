using System;
using System.Collections.Generic;
using System.IO;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Common.Controls;

namespace Teltec.Backup.App.Controls
{
	public class BackupPlanTreeNodeData
	{
		public object Id { get; set; }
		public BackupPlanTreeNode.TypeEnum Type { get; set; }
		public object InfoObject { get; set; }

		private CheckState _State = CheckState.Unchecked;
		public CheckState State
		{
			get { return _State; }
			set { _State = value; }
		}

		private string _Path;
		public string Path
		{
			get
			{
				return _Path != null ? _Path : BuildPath(this);
			}
			set
			{
				_Path = value;
			}
		}

		private static string BuildPath(BackupPlanTreeNode.TypeEnum type, object infoObject)
		{
			switch (type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "type");
				case BackupPlanTreeNode.TypeEnum.FILE:
					return (infoObject as FileInfo).FullName;
				case BackupPlanTreeNode.TypeEnum.FOLDER:
					return (infoObject as DirectoryInfo).FullName;
				case BackupPlanTreeNode.TypeEnum.DRIVE:
					return (infoObject as DriveInfo).Name;
			}
		}
		
		public static string BuildPath(BackupPlanTreeNodeData data)
		{
			if (data.InfoObject == null && data._Path == null)
				return null;

			switch (data.Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "data.Type");
				case BackupPlanTreeNode.TypeEnum.FILE:
				case BackupPlanTreeNode.TypeEnum.FOLDER:
				case BackupPlanTreeNode.TypeEnum.DRIVE:
					return data._Path == null ? BuildPath(data.Type, data.InfoObject) : data._Path;
			}
		}
	}

	public static class BackupPlanTreeNodeDataExtensions
	{
		public static EntryType ToEntryType(this BackupPlanTreeNodeData data)
		{
			switch (data.Type)
			{
				default: throw new ArgumentException("Unhandled BackupPlanTreeNodeData.Type", "tag.type");
				case BackupPlanTreeNode.TypeEnum.DRIVE:
					return EntryType.DRIVE;
				case BackupPlanTreeNode.TypeEnum.FOLDER:
					return EntryType.FOLDER;
				case BackupPlanTreeNode.TypeEnum.FILE:
					return EntryType.FILE;
				//case BackupPlanTreeNode.TypeEnum.FILE_VERSION:
				//	return EntryType.FILE_VERSION;
			}
		}

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
