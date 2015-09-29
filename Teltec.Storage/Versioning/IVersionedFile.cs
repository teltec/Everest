using System;

namespace Teltec.Storage.Versioning
{
	public interface IFileVersion
	{
		string Name { get; set; }
		string Version { get; set; }

		string ToString();
	}

	public class FileVersion : IFileVersion
	{
		public string Name { get; set; }
		public string Version { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	public interface IVersionedFile
	{
		string Path { get; }
		long Size { get; }
		DateTime LastWriteTimeUtc { get; }
		byte[] Checksum { get; }
		IFileVersion Version { get; }
		bool IsVersioned { get; }
		object UserData { get; }
	}
}
