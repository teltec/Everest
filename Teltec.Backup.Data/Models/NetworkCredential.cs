using System;

namespace Teltec.Backup.Data.Models
{
	public class NetworkCredential : BaseEntity<Int32?>, ICloneable, IEquatable<NetworkCredential>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		public const int MountPointMaxLen = 255;
		private string _MountPoint;
		public virtual string MountPoint
		{
			get { return _MountPoint; }
			set { SetField(ref _MountPoint, value); }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}

		public const int LoginMaxLen = 255;
		private string _Login;
		public virtual string Login
		{
			get { return _Login; }
			set { SetField(ref _Login, value); }
		}

		public const int PasswordMaxLen = 255;
		private string _Password; // TODO(jweyrich): Should be a `SecureString` instead?
		public virtual string Password
		{
			get { return _Password; }
			set { SetField(ref _Password, value); }
		}

		public virtual void RevertTo(NetworkCredential other)
		{
			this.Id = other.Id;
			this.MountPoint = other.MountPoint;
			this.Path = other.Path;
			this.Login = other.Login;
			this.Password = other.Password;
		}

		#region ICloneable

		public virtual object Clone()
		{
			// NOTE: This creates a shallow copy, not a deep copy.
			//       REFERRED OBJECTS ARE NOT COPIED!
			return this.MemberwiseClone();
		}

		#endregion

		#region Object overrides

		//
		// REFERENCE: http://nhibernate.info/doc/patternsandpractices/identity-field-equality-and-hash-code.html
		//

		public override bool Equals(object obj)
		{
			NetworkCredential other = obj as NetworkCredential;
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

		public virtual bool Equals(NetworkCredential other)
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

		public static bool operator ==(NetworkCredential x, NetworkCredential y)
		{
			return Equals(x, y);
		}

		public static bool operator !=(NetworkCredential x, NetworkCredential y)
		{
			return !(x == y);
		}

		#endregion
	}
}
