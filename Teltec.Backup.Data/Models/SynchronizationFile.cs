using NLog;
using System;

namespace Teltec.Backup.Data.Models
{
	public class SynchronizationFile : BaseEntity<Int64?>, IEquatable<SynchronizationFile>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		//public SynchronizationFile()
		//{
		//}

		//public SynchronizationFile(Synchronization sync)
		//	: this()
		//{
		//	Synchronization = sync;
		//}

		//public SynchronizationFile(Synchronization sync, string path)
		//	: this(sync)
		//{
		//	Path = path;
		//}

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

		// REFERENCE: http://stackoverflow.com/questions/417142/what-is-the-maximum-length-of-a-url-in-different-browsers
		public const int PathMaxLen = 2000; // We need a larger size to store the URL.
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}

		private long _FileSize;
		public virtual long FileSize
		{
			get { return _FileSize; }
			set { SetField(ref _FileSize, value); }
		}

		private DateTime _LastWrittenAt; // Last date the file was modified.
		public virtual DateTime LastWrittenAt
		{
			get { return _LastWrittenAt; }
			set { SetField(ref _LastWrittenAt, value); }
		}

		//private byte[] _LastChecksum; // SHA-1
		//public virtual byte[] LastChecksum
		//{
		//	get { return _LastChecksum; }
		//	set { SetField(ref _LastChecksum, value); }
		//}

		private DateTime _CreatedAt; // The date this entity was created.
		public virtual DateTime CreatedAt
		{
			get { return _CreatedAt; }
			set { SetField(ref _CreatedAt, value); }
		}

		private DateTime? _UpdatedAt; // Last date this entity was updated.
		public virtual DateTime? UpdatedAt
		{
			get { return _UpdatedAt; }
			set { SetField(ref _UpdatedAt, value); }
		}

		private SynchronizationPathNode _PathNode;
		public virtual SynchronizationPathNode PathNode
		{
			get { return _PathNode; }
			set { _PathNode = value; }
		}

		#region Object overrides

		//
		// REFERENCE: http://nhibernate.info/doc/patternsandpractices/identity-field-equality-and-hash-code.html
		//

		public override bool Equals(object obj)
		{
			SynchronizationFile other = obj as SynchronizationFile;
			return this.Equals(other);
		}

		private int? _oldHashCode;

		public override int GetHashCode()
		{
			// Once we have a hash code we'll never change it
			if (_oldHashCode.HasValue)
				return _oldHashCode.Value;

			bool thisIsTransient = !Id.HasValue;

			// When this instance is transient, we use the base GetHashCode()
			// and remember it, so an instance can NEVER change its hash code.
			if (thisIsTransient)
			{
				_oldHashCode = base.GetHashCode();
				return _oldHashCode.Value;
			}
			return Id.GetHashCode();
		}

		#endregion

		#region IEquatable<T>

		public virtual bool Equals(SynchronizationFile other)
		{
			// If parameter is null, return false.
			if (other == null)
				return false;

			bool otherIsTransient = !other.Id.HasValue;
			bool thisIsTransient = !Id.HasValue;
			if (otherIsTransient && thisIsTransient)
				return ReferenceEquals(other, this);

			return other.Id.Equals(Id);
		}

		#endregion

		#region Operators

		public static bool operator ==(SynchronizationFile x, SynchronizationFile y)
		{
			return Equals(x, y);
		}

		public static bool operator !=(SynchronizationFile x, SynchronizationFile y)
		{
			return !(x == y);
		}

		#endregion
	}
}
