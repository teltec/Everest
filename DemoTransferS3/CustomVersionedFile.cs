using System;
using Teltec.Storage.Versioning;

namespace DemoTransferS3
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

		public CustomVersionedFile(string path)
		{
			Path = path;
		}
	}
}
