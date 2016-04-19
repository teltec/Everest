using System;

namespace Teltec.Backup.App.Controls
{
	public class DriveItem : IEquatable<DriveItem>
	{
		public string Text { get; set; }
		public string LocalDrive { get; set; }
		public string MappedPath { get; set; }
		public bool IsDriveAvailable { get; set; }

		// The `ToString()` method is used by the DataBinding to define which value
		// is going to be bound - It doesn't seem no respect the defined ValueMember.
		public override string ToString()
		{
			return string.IsNullOrEmpty(LocalDrive) ? Text : LocalDrive;
		}

		#region Object overrides

		//
		// REFERENCE: http://nhibernate.info/doc/patternsandpractices/identity-field-equality-and-hash-code.html
		//

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			Type objType = obj.GetType();

			if (objType == this.GetType())
			{
				DriveItem other = obj as DriveItem;
				return this.Equals(obj as DriveItem);
			}
			else if (objType == typeof(string))
			{
				DriveItem other = new DriveItem { LocalDrive = (string)obj };
				return this.Equals(other);
			}

			return false;
		}

		private int? _oldHashCode;

		public override int GetHashCode()
		{
			// Once we have a hash code we'll never change it
			if (_oldHashCode.HasValue)
				return _oldHashCode.Value;

			// Use the [LocalDrive|Text].GetHashCode() and remember it, so an instance can NEVER change its hash code.
			_oldHashCode = string.IsNullOrEmpty(LocalDrive)
				? Text.GetHashCode()
				: LocalDrive.GetHashCode();
			return _oldHashCode.Value;
		}

		#endregion

		#region IEquatable<T>

		public virtual bool Equals(DriveItem other)
		{
			// If parameter is null, return false.
			if (other == null)
				return false;

			return ReferenceEquals(other, this)
				|| (other.LocalDrive != null && other.LocalDrive.Equals(LocalDrive))
				|| (other.Text != null && other.Text.Equals(Text));
		}

		#endregion

		#region Operators

		public static bool operator ==(DriveItem x, DriveItem y)
		{
			return Equals(x, y);
		}

		public static bool operator !=(DriveItem x, DriveItem y)
		{
			return !(x == y);
		}

		#endregion
	}
}
