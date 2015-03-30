using System;
using Teltec.Common.Forms;

namespace Teltec.Backup.App.Models
{
	public enum EntryType
	{
		DRIVE = 1,
		FOLDER = 2,
		FILE = 3,
	}

	public static class EntryTypeExtensions
	{
		public static FileSystemTreeNodeTag.InfoType ToInfoType(this EntryType obj)
		{
			switch (obj)
			{
				default: throw new ArgumentException("Unhandled EntryType", "obj");
				case EntryType.DRIVE:
					return FileSystemTreeNodeTag.InfoType.DRIVE;
				case EntryType.FOLDER:
					return FileSystemTreeNodeTag.InfoType.FOLDER;
				case EntryType.FILE:
					return FileSystemTreeNodeTag.InfoType.FILE;
			}
		}
	}
}
