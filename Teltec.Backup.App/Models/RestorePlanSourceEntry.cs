using System;
using System.Collections.Generic;
using Teltec.Common.Forms;

namespace Teltec.Backup.App.Models
{
	public class RestorePlanSourceEntry : BaseEntity<Int64?>
	{
		//public RestorePlanSourceEntry()
		//{
		//}

		//public RestorePlanSourceEntry(RestorePlan plan, EntryType type, string path) : this()
		//{
		//	RestorePlan = plan;
		//	Type = type;
		//	Path = path;
		//}

		//public RestorePlanSourceEntry(RestorePlan plan, FileSystemTreeNodeTag tag)
		//	: this(plan, tag.ToEntryType(), tag.Path)
		//{
		//}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private RestorePlan _RestorePlan;
		public virtual RestorePlan RestorePlan
		{
			get { return _RestorePlan; }
			set { SetField(ref _RestorePlan, value); }
		}

		private EntryType _Type;
		public virtual EntryType Type
		{
			get { return _Type; }
			set { SetField(ref _Type, value); }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}
	}
}
