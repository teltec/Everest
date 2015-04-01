using System;
using System.Collections.Generic;
using System.IO;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Common.Controls;

namespace Teltec.Backup.App.Controls
{
	public class FileSystemTreeNodeData
	{
		public object Id { get; set; }
		public FileSystemTreeNode.TypeEnum Type { get; set; }
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
		
		private static string BuildPath(FileSystemTreeNode.TypeEnum type, object infoObject)
		{
			switch (type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "type");
				case FileSystemTreeNode.TypeEnum.FILE:
					return (infoObject as FileInfo).FullName;
				case FileSystemTreeNode.TypeEnum.FOLDER:
					return (infoObject as DirectoryInfo).FullName;
				case FileSystemTreeNode.TypeEnum.DRIVE:
					return (infoObject as DriveInfo).Name;
			}
		}
		
		public static string BuildPath(FileSystemTreeNodeData data)
		{
			if (data.InfoObject == null && data._Path == null)
				return null;

			switch (data.Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "data.Type");
				case FileSystemTreeNode.TypeEnum.FILE:
				case FileSystemTreeNode.TypeEnum.FOLDER:
				case FileSystemTreeNode.TypeEnum.DRIVE:
					return data._Path == null ? BuildPath(data.Type, data.InfoObject) : data._Path;
			}
		}
	}

	public static class FileSystemTreeNodeDataExtensions
	{
		public static EntryType ToEntryType(this FileSystemTreeNodeData data)
		{
			switch (data.Type)
			{
				default: throw new ArgumentException("Unhandled FileSystemTreeNodeData.Type", "tag.type");
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
