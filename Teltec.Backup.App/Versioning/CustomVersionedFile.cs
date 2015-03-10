using System;
using System.IO;
using Teltec.Storage;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Versioning
{
	public sealed class CustomVersionedFile : IVersionedFile
	{
		public string Path
		{
			get;
			private set;
		}

		public long Size
		{
			get;
			private set;
		}

		public DateTime LastWriteTimeUtc
		{
			get;
			private set;
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

		public CustomVersionedFile(string path, long size, DateTime lastWriteTimeUtc, byte[] checksum, IFileVersion version)
		{
			Path = path;
			Size = size;
			LastWriteTimeUtc = lastWriteTimeUtc;
			Checksum = checksum;
			Version = version;
		}

		public CustomVersionedFile(string path, long size, DateTime lastWriteTimeUtc)
			: this(path, size, lastWriteTimeUtc, null, null) 
		{
		}

		public CustomVersionedFile(FileInfo file)
			: this(file.FullName, file.Length, file.LastWriteTimeUtc)
		{
		}
	}
}
