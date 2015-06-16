using System;
using System.Collections.Generic;

namespace Teltec.Backup.Data.Models
{
	public class SynchronizationPathNode : BaseEntity<Int64?>
	{
		public SynchronizationPathNode()
		{
		}

		public SynchronizationPathNode(SynchronizationFile syncFile, EntryType type, string name, string path, SynchronizationPathNode parent)
			: this()
		{
			Synchronization = syncFile.Synchronization;
			Type = type;
			// Only assign `SyncFile` if this is for a node that represents a FILE.
			if (Type == EntryType.FILE)
				SyncFile = syncFile;
			Name = name;
			Path = path;
			Parent = parent;
		}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private Synchronization _Synchronization;
		public virtual Synchronization Synchronization
		{
			get { return _Synchronization; }
			set { _Synchronization = value; }
		}

		private SynchronizationPathNode _Parent;
		public virtual SynchronizationPathNode Parent
		{
			get { return _Parent; }
			set { _Parent = value; }
		}

		private EntryType _Type;
		public virtual EntryType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

		public const int NameMaxLen = 255;
		private string _Name;
		public virtual string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { _Path = value; }
		}

		private IList<SynchronizationPathNode> _SubNodes = new List<SynchronizationPathNode>();
		public virtual IList<SynchronizationPathNode> SubNodes
		{
			get { return _SubNodes; }
			protected set { SetField(ref _SubNodes, value); }
		}

		private SynchronizationFile _SyncFile;
		public virtual SynchronizationFile SyncFile
		{
			get { return _SyncFile; }
			protected set { SetField(ref _SyncFile, value); }
		}
	}
}
