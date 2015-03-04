using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Storage.Versioning
{
	public interface IFileVersion
	{
		string Version { get; set; }
		DateTime Timestamp { get; set; }

		string ToString();
	}

	public class FileVersion : IFileVersion
	{
		DateTime _Timestamp = DateTime.UtcNow;
		
		public string Version { get; set; }
		public DateTime Timestamp
		{
			get { return _Timestamp; }
			set { _Timestamp = value; }
		}

		public string ToString()
		{
			return Version;
		}
	}

	public interface IVersionedFile<FileT> where FileT : class
	{
		FileT File { get; set; }
		IFileVersion Version { get; set; }
		bool IsVersioned { get; }
	}

	public abstract class VersionedFile<FileT> where FileT : class
	{
		public FileT File { get; private set; }
		public IFileVersion Version { get; private set; }
		public bool IsVersioned { get { return Version != null; } }

		public VersionedFile(FileT file, IFileVersion version)
		{
			File = file;
			Version = version;
		}

		public VersionedFile(FileT file)
			: this(file, null)
		{
		}
	}
}
