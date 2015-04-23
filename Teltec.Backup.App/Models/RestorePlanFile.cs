using NLog;
using System;
using Teltec.Backup.App.Versioning;

namespace Teltec.Backup.App.Models
{
	//public enum RestoreFileStatus
	//{
	//	UNCHANGED = 0,
	//	ADDED = 1, // File was added to the backup.
	//	REMOVED = 2, // File was removed from the backup.
	//	DELETED = 3, // File was deleted and will no longer be backup'ed - it doesn't mean it will be deleted from backups.
	//	MODIFIED = 4, // File was modified.
	//}

	public class RestorePlanFile : BaseEntity<Int64?>, IEquatable<RestorePlanFile>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public RestorePlanFile()
		{
		}

		public RestorePlanFile(RestorePlan plan)
			: this()
		{
			RestorePlan = plan;
		}

		public RestorePlanFile(RestorePlan plan, string path)
			: this(plan)
		{
			Path = path;
		}

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
			protected set { _RestorePlan = value; }
		}

		public const int PathMaxLen = RestorePlanSourceEntry.PathMaxLen;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}

		private BackupPlanPathNode _PathNode;
		public virtual BackupPlanPathNode PathNode
		{
			get { return _PathNode; }
			set { _PathNode = value; }
		}

		//private BackupFileStatus _LastStatus;
		//public virtual BackupFileStatus LastStatus
		//{
		//	get { return _LastStatus; }
		//	set { SetField(ref _LastStatus, value); }
		//}

		private DateTime _CreatedAt; // The date this entity was created.
		public virtual DateTime CreatedAt
		{
			get { return _CreatedAt; }
			set { SetField(ref _CreatedAt, value); }
		}

		//private DateTime? _UpdatedAt; // Last date this entity was updated.
		//public virtual DateTime? UpdatedAt
		//{
		//	get { return _UpdatedAt; }
		//	set { SetField(ref _UpdatedAt, value); }
		//}

		private CustomVersionedFile _VersionedFile;
		public virtual CustomVersionedFile VersionedFile // Non-persistent property.
		{
			get { return _VersionedFile; }
			set { SetField(ref _VersionedFile, value); }
		}

		#region Object overrides

		//
		// REFERENCE: http://nhibernate.info/doc/patternsandpractices/identity-field-equality-and-hash-code.html
		//

		public override bool Equals(object obj)
		{
			RestorePlanFile other = obj as RestorePlanFile;
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

		public virtual bool Equals(RestorePlanFile other)
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

		public static bool operator ==(RestorePlanFile x, RestorePlanFile y)
		{
			return Equals(x, y);
		}

		public static bool operator !=(RestorePlanFile x, RestorePlanFile y)
		{
			return !(x == y);
		}

		#endregion
	}
}
