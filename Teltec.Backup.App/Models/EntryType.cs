using System;
using Teltec.Backup.App.Controls;
using Teltec.FileSystem;

namespace Teltec.Backup.App.Models
{
	public enum EntryType
	{
		DRIVE = 1,
		FOLDER = 2,
		FILE = 3,
		FILE_VERSION = 4,
	}

	public static class EntryTypeExtensions
	{
		public static TypeEnum ToTypeEnum(this EntryType value)
		{
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Unhandled {0} value", typeof(EntryType).FullName), "value");
				case EntryType.DRIVE: return TypeEnum.DRIVE;
				case EntryType.FOLDER: return TypeEnum.FOLDER;
				case EntryType.FILE: return TypeEnum.FILE;
				case EntryType.FILE_VERSION: return TypeEnum.FILE_VERSION;
			}
		}

		public static EntryType ToEntryType(this PathNode.TypeEnum value)
		{
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Unhandled {0} value", typeof(PathNode.TypeEnum).FullName), "value");
				case PathNode.TypeEnum.DRIVE: return EntryType.DRIVE;
				case PathNode.TypeEnum.FOLDER: return EntryType.FOLDER;
				case PathNode.TypeEnum.FILE: return EntryType.FILE;
			}
		}
	}
}
