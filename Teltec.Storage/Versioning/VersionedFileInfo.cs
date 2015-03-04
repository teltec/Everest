using System.IO;

namespace Teltec.Storage.Versioning
{
	public class VersionedFileInfo : VersionedFile<FileInfo>
	{
		public VersionedFileInfo(string path)
			: this(path, null)
		{
		}

		public VersionedFileInfo(FileInfo file)
			: this(file, null)
		{
		}

		public VersionedFileInfo(string path, IFileVersion version)
			: this(new FileInfo(path), version)
		{
		}

		public VersionedFileInfo(FileInfo file, IFileVersion version)
			: base(file, version)
		{
		}
	}
}
