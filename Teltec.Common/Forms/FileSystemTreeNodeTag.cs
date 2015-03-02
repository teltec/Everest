using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Common.Forms
{
	public class FileSystemTreeNodeTag
	{
		public enum InfoType
		{
			LOADING = 0,
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
		}

		//public FileSystemTreeNodeTag(
		//	object id, InfoType infoType, string path, CheckState state = CheckState.Unchecked)
		//{
		//	Id = id;
		//	Type = infoType;
		//	Path = path;
		//	State = state;
		//}

		//public FileSystemTreeNodeTag(
		//	object id, InfoType infoType, object infoObject, CheckState state = CheckState.Unchecked)
		//{
		//	Id = Id;
		//	Type = infoType;
		//	Path = BuildPath(infoType, infoObject);
		//	InfoObject = infoObject;
		//	State = state;
		//}

		public object Id { get; set; }
		public InfoType Type { get; set; }
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
		
		public static string BuildPath(InfoType type, object infoObject)
		{
			switch (type)
			{
				default:
					throw new ArgumentException("Unhandled InfoType", "type");
				case InfoType.FILE:
					return (infoObject as FileInfo).FullName;
				case InfoType.FOLDER:
					return (infoObject as DirectoryInfo).FullName;
				case InfoType.DRIVE:
					return (infoObject as DriveInfo).Name;
			}
		}
		
		public static string BuildPath(FileSystemTreeNodeTag tag)
		{
			if (tag.InfoObject == null && tag._Path == null)
				return null;

			switch (tag.Type)
			{
				default:
					throw new ArgumentException("Unhandled InfoType", "tag.Type");
				case InfoType.FILE:
				case InfoType.FOLDER:
				case InfoType.DRIVE:
					return tag._Path == null ? BuildPath(tag.Type, tag.InfoObject) : tag._Path;
			}
		}
	}
}
