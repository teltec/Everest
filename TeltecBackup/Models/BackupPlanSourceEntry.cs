using System;
using System.Collections.Generic;
using Teltec.Common;
using Teltec.Common.Forms;

namespace Teltec.Backup.Models
{
	public class BackupPlanSourceEntry : ObservableObject
	{
		public enum EntryType
		{
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
		}

		private int _Id;
		public int Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		public BackupPlanSourceEntry()
		{
		}

		public BackupPlanSourceEntry(EntryType type, string path) : this()
		{
			Type = type;
			Path = path;
		}

		public BackupPlanSourceEntry(FileSystemTreeNodeTag tag)
			: this(tag.ToEntryType(), tag.Path)
		{
		}

		private EntryType _Type;
		public EntryType Type
		{
			get { return _Type; }
			set { SetField(ref _Type, value); }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}
	}

	public static class EntryTypeExtensions
	{
		public static FileSystemTreeNodeTag.InfoType ToInfoType(
			this BackupPlanSourceEntry.EntryType obj)
		{
			switch (obj)
			{
				case BackupPlanSourceEntry.EntryType.DRIVE:
					return FileSystemTreeNodeTag.InfoType.DRIVE;
				case BackupPlanSourceEntry.EntryType.FOLDER:
					return FileSystemTreeNodeTag.InfoType.FOLDER;
				case BackupPlanSourceEntry.EntryType.FILE:
					return FileSystemTreeNodeTag.InfoType.FILE;
				default:
					throw new ArgumentException("Unhandled EntryType", "obj");
			}
		}
	}

	public static class TreeNodeTagExtensions
	{
		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `BackupPlanSourceEntry`.
		public static List<BackupPlanSourceEntry> ToBackupPlanSourceEntry(this List<FileSystemTreeNodeTag> tags)
		{
			List<BackupPlanSourceEntry> entries = new List<BackupPlanSourceEntry>(tags.Count);
			foreach (Teltec.Common.Forms.FileSystemTreeNodeTag tag in tags)
				entries.Add(new BackupPlanSourceEntry(tag));
			return entries;
		}

		public static BackupPlanSourceEntry.EntryType ToEntryType(this FileSystemTreeNodeTag tag)
		{
			switch (tag.Type)
			{
				default: throw new ArgumentException("type", "Unhandled TreeNodeTag.InfoType");
				case FileSystemTreeNodeTag.InfoType.DRIVE:
					return BackupPlanSourceEntry.EntryType.DRIVE;
				case FileSystemTreeNodeTag.InfoType.FOLDER:
					return BackupPlanSourceEntry.EntryType.FOLDER;
				case FileSystemTreeNodeTag.InfoType.FILE:
					return BackupPlanSourceEntry.EntryType.FILE;
			}
		}
	}
}
