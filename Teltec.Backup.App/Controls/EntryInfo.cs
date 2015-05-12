using Teltec.Backup.Data.FileSystem;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Controls
{
	public class EntryInfo
	{
		public TypeEnum Type { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public IFileVersion Version { get; set; }

		public EntryInfo(TypeEnum type, string name, string path)
		{
			Type = type;
			Name = name;
			Path = path;
		}

		public EntryInfo(TypeEnum type, string name, string path, IFileVersion version)
			: this(type, name, path)
		{
			Version = version;
		}
	}
}
