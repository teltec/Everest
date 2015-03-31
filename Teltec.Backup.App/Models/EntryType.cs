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
		public static FileSystemTreeNode.TypeEnum ToTypeEnum(this EntryType obj)
		{
			switch (obj)
			{
				default: throw new ArgumentException("Unhandled EntryType", "obj");
				case EntryType.DRIVE:
					return FileSystemTreeNode.TypeEnum.DRIVE;
				case EntryType.FOLDER:
					return FileSystemTreeNode.TypeEnum.FOLDER;
				case EntryType.FILE:
					return FileSystemTreeNode.TypeEnum.FILE;
			}
		}
	}
}
