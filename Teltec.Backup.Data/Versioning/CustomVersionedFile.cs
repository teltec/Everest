using System;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.Data.Versioning
{
	public sealed class CustomVersionedFile : IVersionedFile
	{
		public string Path
		{
			get;
			set;
		}

		public long Size
		{
			get;
			set;
		}

		public DateTime LastWriteTimeUtc
		{
			get;
			set;
		}

		public byte[] Checksum
		{
			get;
			set;
		}

		public IFileVersion Version
		{
			get;
			set;
		}

		public bool IsVersioned
		{
			get { return Version != null; }
		}

		public object UserData
		{
			get;
			set;
		}

		public CustomVersionedFile()
		{
		}

		public CustomVersionedFile(string path)
			: this()
		{
			Path = path;
		}
	}
}
