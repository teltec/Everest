using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;

namespace Teltec.Backup.Data.Models
{
	public enum BackupFileStatus
	{
		UNCHANGED = 0,
		ADDED = 1, // File was added to the backup.
		REMOVED = 2, // File was removed from the backup.
		DELETED = 3, // File was deleted and will no longer be backup'ed - it doesn't mean it will be deleted from backups.
		MODIFIED = 4, // File was modified.
	}

	public class BackupPlanFile : BaseEntity<Int64?>, IEquatable<BackupPlanFile>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public BackupPlanFile()
		{
			_Versions = new List<BackupedFile>();
		}

		public BackupPlanFile(StorageAccount account)
			: this()
		{
			_StorageAccountType = account.Type;
			_StorageAccount = account;
		}

		public BackupPlanFile(StorageAccount account, string path)
			: this(account)
		{
			_Path = path;
		}

		public BackupPlanFile(BackupPlan plan)
			: this(plan.StorageAccount)
		{
			_BackupPlan = plan;
		}

		public BackupPlanFile(BackupPlan plan, string path)
			: this(plan.StorageAccount)
		{
			_BackupPlan = plan;
			_Path = path;
		}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private BackupPlan _BackupPlan;
		public virtual BackupPlan BackupPlan
		{
			get { return _BackupPlan; }
			set { SetField(ref _BackupPlan, value); }
		}

		#region Account

		private EStorageAccountType _StorageAccountType;
		public virtual EStorageAccountType StorageAccountType
		{
			get { return _StorageAccountType; }
			set { SetField(ref _StorageAccountType, value); }
		}

		//private int _StorageAccountId;
		//public virtual int StorageAccountId
		//{
		//	get { return _StorageAccountId; }
		//	set { SetField(ref _StorageAccountId, value); }
		//}

		//public static ICloudStorageAccount GetStorageAccount(BackupPlan plan, ICloudStorageAccount dao)
		//{
		//	switch (plan.StorageAccountType)
		//	{
		//		default:
		//			throw new ArgumentException("Unhandled StorageAccountType", "plan");
		//		case EStorageAccountType.AmazonS3:
		//			return dao.Get(plan.StorageAccountId);
		//	}
		//}

		private StorageAccount _StorageAccount;
		public virtual StorageAccount StorageAccount
		{
			get { return _StorageAccount; }
			set { SetField(ref _StorageAccount, value); }
		}

		#endregion

		public const int PathMaxLen = BackupPlanSourceEntry.PathMaxLen;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, StringUtils.NormalizeUsingPreferredForm(value)); }
		}

		private long _LastSize;
		public virtual long LastSize
		{
			get { return _LastSize; }
			set { SetField(ref _LastSize, value); }
		}

		private DateTime _LastWrittenAt; // Last date the file was modified.
		public virtual DateTime LastWrittenAt
		{
			get { return _LastWrittenAt; }
			set { SetField(ref _LastWrittenAt, value); }
		}

		private byte[] _LastChecksum; // SHA-1
		public virtual byte[] LastChecksum
		{
			get { return _LastChecksum; }
			set { SetField(ref _LastChecksum, value); }
		}

		private BackupFileStatus _LastStatus;
		public virtual BackupFileStatus LastStatus
		{
			get { return _LastStatus; }
			set
			{
				SetField(ref _LastStatus, value);
				PreviousLastStatus = value; // Update `PreviousLastStatus` as well.
			}
		}

		private BackupFileStatus _PreviousLastStatus;
		public virtual BackupFileStatus PreviousLastStatus
		{
			get { return _PreviousLastStatus; }
			protected set { _PreviousLastStatus = value; }
		}

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

		private BackupPlanPathNode _PathNode;
		public virtual BackupPlanPathNode PathNode
		{
			get { return _PathNode; }
			set { _PathNode = value; }
		}

		private IList<BackupedFile> _Versions;
		public virtual IList<BackupedFile> Versions
		{
			get { return _Versions; }
			protected set { _Versions = value; }
		}

		#region Object overrides

		//
		// REFERENCE: http://nhibernate.info/doc/patternsandpractices/identity-field-equality-and-hash-code.html
		//

		public override bool Equals(object obj)
		{
			BackupPlanFile other = obj as BackupPlanFile;
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

		public virtual bool Equals(BackupPlanFile other)
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

		public static bool operator ==(BackupPlanFile x, BackupPlanFile y)
		{
			return Equals(x, y);
		}

		public static bool operator !=(BackupPlanFile x, BackupPlanFile y)
		{
			return !(x == y);
		}

		#endregion

		#region Debug helpers

		public virtual string DumpMe()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("BackupPlanFile {{ ");
			sb.AppendFormat("Id = {0}, ", this.Id.HasValue ? this.Id.Value.ToString() : "NULL");
			sb.AppendFormat("BackupPlan = {{ Id = {0} }}, ", this.BackupPlan.Id.HasValue ? this.BackupPlan.Id.ToString() : "NULL");
			sb.AppendFormat("Path = {0}, ", this.Path);
			sb.AppendFormat("LastSize = {0}, ", this.LastSize);
			sb.AppendFormat("LastWrittenAt = {0}, ", this.LastWrittenAt);
			sb.AppendFormat("LastChecksum = {0}, ", this.LastChecksum.ToString());
			sb.AppendFormat("LastStatus = {0}, ", this.LastStatus);
			sb.AppendFormat("CreatedAt = {0}, ", this.CreatedAt);
			sb.AppendFormat("UpdatedAt = {0}, ", this.UpdatedAt.HasValue ? this.UpdatedAt.Value.ToString() : "NULL");
			if (this.PathNode != null)
			{
				bool hasPlanFile = this.PathNode.PlanFile != null;
				bool hasPlanFileId = hasPlanFile && this.PathNode.PlanFile.Id.HasValue;
				sb.AppendFormat("PathNode = {{ Id = {0}, Name = {1}, Parent = {2}, Type = {3}, HasPlanFile = {4}, PlanFile = {{ Id = {5} }} }}",
					this.PathNode.Id.HasValue ? this.PathNode.Id.ToString() : "NULL",
					this.PathNode.Name,
					this.PathNode.Parent,
					this.PathNode.Type,
					hasPlanFile,
					hasPlanFile && hasPlanFileId ? this.PathNode.PlanFile.Id.ToString() : "NULL"
				);
			}
			else
			{
				sb.AppendFormat("PathNode = NULL");
			}
			sb.Append(" }");

			return sb.ToString();
		}

		#endregion
	}
}
