using System;
using System.Collections.Generic;
using Teltec.Common;

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

		public BackupPlanSourceEntry(EntryType type, string path)
		{
			Type = type;
			Path = path;
		}

		public BackupPlanSourceEntry(Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag tag)
		{
			Type = tag.ToEntryType();
			Path = tag.Path;
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

	public static class TreeNodeTagExtensions
	{
		// Convert collection of `FileSystemTreeView.TreeNodeTag` to `BackupPlanSourceEntry`.
		public static IList<BackupPlanSourceEntry> ToBackupPlanSourceEntry(
			this IList<Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag> tags)
		{
			IList<BackupPlanSourceEntry> entries = new List<BackupPlanSourceEntry>(tags.Count);
			foreach (Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag tag in tags)
				entries.Add(new BackupPlanSourceEntry(tag));
			return entries;
		}

		public static BackupPlanSourceEntry.EntryType ToEntryType(
			this Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag tag)
		{
			switch (tag.Type)
			{
				default: throw new ArgumentException("type", "Unhandled TreeNodeTag.InfoType");
				case Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag.InfoType.DRIVE:
					return BackupPlanSourceEntry.EntryType.DRIVE;
				case Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag.InfoType.FOLDER:
					return BackupPlanSourceEntry.EntryType.FOLDER;
				case Teltec.Common.Forms.FileSystemTreeView.TreeNodeTag.InfoType.FILE:
					return BackupPlanSourceEntry.EntryType.FILE;
			}
		}
	}
}
