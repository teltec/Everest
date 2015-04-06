using System;
using Teltec.Backup.App.Models;
using Teltec.Common.Controls;

namespace Teltec.Backup.App.Controls
{
	public abstract class EntryTreeNodeData
	{
		public object Id { get; set; }
		public TypeEnum Type { get; set; }
		public string Name { get; set; } // Depends on `InfoObject`
		public string Path { get; set; } // Depends on `InfoObject`

		private EntryInfo _InfoObject;
		public EntryInfo InfoObject
		{
			get { return _InfoObject; }
			set
			{
				_InfoObject = value;
				Type = _InfoObject.Type;
				UpdateProperties();
			}
		}

		private CheckState _State = CheckState.Unchecked;
		public CheckState State
		{
			get { return _State; }
			set { _State = value; }
		}

		protected abstract void UpdateProperties();

		public EntryType ToEntryType()
		{
			switch (Type)
			{
				default: throw new ArgumentException("Unhandled TypeEnum", "Type");
				case TypeEnum.DRIVE: return EntryType.DRIVE;
				case TypeEnum.FOLDER: return EntryType.FOLDER;
				case TypeEnum.FILE: return EntryType.FILE;
				case TypeEnum.FILE_VERSION: return EntryType.FILE_VERSION;
			}
		}
	}
}
