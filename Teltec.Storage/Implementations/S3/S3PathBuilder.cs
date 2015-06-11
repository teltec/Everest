using System;
using System.IO;
using Teltec.FileSystem;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Implementations.S3
{
	public class S3PathBuilder : PathBuilder
	{
		#region Local

		public static readonly char LocalDirectorySeparatorChar = Path.DirectorySeparatorChar;

		public override string CombineLocalPath(string localBaseDirectory, params string[] relativePaths)
		{
			string path = localBaseDirectory + LocalDirectorySeparatorChar + string.Join(LocalDirectorySeparatorChar.ToString(), relativePaths);
			return path;
		}

		// Convert from
		//	[<RemoteRootDirectory>/][<drive>:/][<directories>/][<filename>:/<version>/][<filename>]
		// To
		//	[<LocalRootDirectory>\][<drive>[:]\][<directories>\][<filename>]
		public override string BuildLocalPath(string remotePath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Remote

		public static readonly char RemoteDirectorySeparatorChar = '/';
		public static readonly char RemoteVersionPostfixChar = ':';

		public override string RemoteManifestDirectory
		{
			get
			{
				return CombineRemotePath(RemoteRootDirectory, ".metadata");
			}
		}

		public override string CombineRemotePath(string remoteBaseDirectory, params string[] relativePaths)
		{
			string path = remoteBaseDirectory + RemoteDirectorySeparatorChar + string.Join(RemoteDirectorySeparatorChar.ToString(), relativePaths);
			return path;
		}

		// Convert from
		//	[<drive>:\][<directories>\][<filename>]
		// To
		//	[<RemoteRootDirectory>/][<drive>:/][<directories>/][<filename>]
		public override string BuildRemotePath(string localPath)
		{
			return BuildVersionedRemotePath(localPath, null);
		}

		// Convert from
		//	[<drive>:\][<directories>\][<filename>]
		// To
		//	[<RemoteRootDirectory>/][<drive>:/][<directories>/][<filename>:/<version>/][<filename>]
		public override string BuildVersionedRemotePath(string localPath, IFileVersion version)
		{
			PathComponents comps = new PathComponents(localPath);

			string result = "";

			if (!string.IsNullOrEmpty(RemoteRootDirectory))
			{
				// <RootDirectory>/
				if (RemoteRootDirectory.EndsWith("/"))
					result += RemoteRootDirectory;
				else
					result += RemoteRootDirectory + RemoteDirectorySeparatorChar;
			}

			if (comps.HasDrive)
				// <drive>:/
				result += comps.Drive + RemoteVersionPostfixChar + RemoteDirectorySeparatorChar;

			if (comps.HasDirectories)
				// <directories>/
				result += string.Join(RemoteDirectorySeparatorChar.ToString(), comps.Directories) + RemoteDirectorySeparatorChar;

			if (comps.HasFileName)
			{
				if (version != null)
				{
					result += comps.FileName		// <filename>
						+ RemoteVersionPostfixChar		// <filename>:
						+ RemoteDirectorySeparatorChar	// <filename>:/
						+ version.Version			// <filename>:/<version>
						+ RemoteDirectorySeparatorChar;	// <filename>:/<version>/
				}
				result += comps.FileName;			// [<filename>:/<version>/]<filename>
			}

			return result; // [<RootDirectory>/][<drive>:/][<directories>/][<filename>:/<version>/][<filename>]
		}

		private string BuildRemoteKeyPrefix(string filepath)
		{
			// Examples:
			//   "C:\foo\bar"       -> "foo\bar"
			//   "\\remote\foo\bar" -> "bar" (given that the network share is "\\remote\foo")
			return filepath.Substring(Path.GetPathRoot(filepath).Length).Replace('\\', RemoteDirectorySeparatorChar);
		}

		#endregion
	}
}
