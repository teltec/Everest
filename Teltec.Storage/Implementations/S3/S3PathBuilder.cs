using System;
using System.IO;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Implementations.S3
{
	public class S3PathBuilder : PathBuilder
	{
		public readonly string DirectorySeparator = "/";
		public readonly string VersionPostfix = ":";

		public override string BuildPath(string path)
		{
			return BuildVersionedPath(path, null);
		}

		public override string BuildVersionedPath(string path, IFileVersion version)
		{
			PathComponents comps = new PathComponents(path);

			string result = "";

			if (!string.IsNullOrEmpty(RootDirectory))
				// <RootDirectory>/
				result += RootDirectory + DirectorySeparator;

			if (comps.HasDrive)
				// <drive>:/
				result += comps.Drive + VersionPostfix + DirectorySeparator;

			if (comps.HasDirectories)
				// <directories>/
				result += string.Join(DirectorySeparator, comps.Directories) + DirectorySeparator;

			if (comps.HasFileName)
			{
				if (version != null)
				{
					result += comps.FileName	// <filename>
						+ VersionPostfix		// <filename>:
						+ DirectorySeparator	// <filename>:/
						+ version.Version		// <filename>:/<version>
						+ DirectorySeparator;	// <filename>:/<version>/
				}
				result += comps.FileName;		// [<filename>:/<version>/]<filename>
			}

			return result; // [<RootDirectory>/][<drive>:/][<directories>/][<filename>:/<version>/][<filename>]
		}

		private string BuildKeyPrefix(string filepath)
		{
			// Examples:
			//   "C:\foo\bar"       -> "foo\bar"
			//   "\\remote\foo\bar" -> "bar" (given that the network share is "\\remote\foo")
			return filepath.Substring(Path.GetPathRoot(filepath).Length).Replace('\\', '/');
		}
	}
}
