using System;
using Teltec.Backup.App.Controls;

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
		public static TypeEnum ToTypeEnum(this EntryType obj)
		{
			switch (obj)
			{
				default: throw new ArgumentException("Unhandled EntryType", "obj");
				case EntryType.DRIVE: return TypeEnum.DRIVE;
				case EntryType.FOLDER: return TypeEnum.FOLDER;
				case EntryType.FILE: return TypeEnum.FILE;
				case EntryType.FILE_VERSION: return TypeEnum.FILE_VERSION;
			}
		}
	}
}
