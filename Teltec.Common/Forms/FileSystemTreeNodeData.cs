using System;
using System.IO;

namespace Teltec.Common.Forms
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
		
		public static string BuildPath(FileSystemTreeNodeData tag)
		{
			if (tag.InfoObject == null && tag._Path == null)
				return null;

			switch (tag.Type)
			{
				default:
					throw new ArgumentException("Unhandled TypeEnum", "tag.Type");
				case FileSystemTreeNode.TypeEnum.FILE:
				case FileSystemTreeNode.TypeEnum.FOLDER:
				case FileSystemTreeNode.TypeEnum.DRIVE:
					return tag._Path == null ? BuildPath(tag.Type, tag.InfoObject) : tag._Path;
			}
		}
	}
}
