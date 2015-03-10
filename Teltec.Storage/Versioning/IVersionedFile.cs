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

		string ToString();
	}

	public class FileVersion : IFileVersion
	{
		public string Version { get; set; }

		public override string ToString()
		{
			return Version;
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
