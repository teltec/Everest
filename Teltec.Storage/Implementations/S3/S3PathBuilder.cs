using NUnit.Framework;
using System;
using System.Collections.Generic;
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
			string dirSeparator = LocalDirectorySeparatorChar.ToString();
			string path = localBaseDirectory + dirSeparator + string.Join(dirSeparator, relativePaths);
			if (!path.EndsWith(dirSeparator))
				path += dirSeparator;
			return path;
		}

		// Convert from
		//	[<RemoteRootDirectory>/][<drive>:/][<directories>/][<filename>:/<version>/][<filename>]
		// To
		//	[<LocalRootDirectory>\][<drive>[:]\][<directories>\][<filename>]
		public override string BuildLocalPath(string remotePath, out string outVersion)
		{
			Assert.IsNotNullOrEmpty(remotePath);
			Assert.IsNotNullOrEmpty(RemoteRootDirectory);

			string remoteBaseDir = RemoteRootDirectory
				+ (RemoteRootDirectory.EndsWith(RemoteDirectorySeparatorChar.ToString())
					? string.Empty : RemoteDirectorySeparatorChar.ToString());

			if (!remotePath.StartsWith(remoteBaseDir))
				throw new ArgumentException(string.Format("Value MUST start with {0}", remoteBaseDir), "remotePath");

			string partialRemotePath = remotePath.Substring(remoteBaseDir.Length);
			string[] remoteParts = partialRemotePath.Split(RemoteDirectorySeparatorChar);

			bool hasDrive = false;
			bool hasDirectories = false;
			bool hasVersion = false;
			bool hasFilename = false;

			string localDrive = string.Empty;
			List<string> localDirectories = new List<string>();
			string localFilename = string.Empty;
			string localVersion = string.Empty;

			int index = 0;

			// Drive
			localDrive = remoteParts[index++];
			hasDrive = true;

			// Folders
			while (index < remoteParts.Length)
			{
				string temp = remoteParts[index];
				if (temp.EndsWith(RemoteVersionPostfixChar.ToString()))
					break;
				index++;
				localDirectories.Add(temp);
				hasDirectories = true;
			}

			// Skip "filename:"
			if (index < remoteParts.Length)
			{
				index++;
			}

			// Version
			if (index < remoteParts.Length)
			{
				outVersion = remoteParts[index++];
				hasVersion = true;
			}
			else
			{
				outVersion = null;
			}

			// Filename
			if (index < remoteParts.Length)
			{
				localFilename = remoteParts[index++];
				hasFilename = true;
			}

			if (index != remoteParts.Length || !hasDrive /* || !hasDirectories */ || !hasVersion || !hasFilename)
				throw new IndexOutOfRangeException(string.Format("Failed to parse S3 path - {0}", remotePath));

			string localPath = (hasDrive ? localDrive + (hasDirectories || hasFilename ? LocalDirectorySeparatorChar.ToString() : string.Empty) : string.Empty)
				+ (hasDirectories ? string.Join(LocalDirectorySeparatorChar.ToString(), localDirectories) + LocalDirectorySeparatorChar : string.Empty)
				+ (hasFilename ? localFilename : string.Empty);

			return localPath;
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
			string dirSeparator = RemoteDirectorySeparatorChar.ToString();
			string path = remoteBaseDirectory + dirSeparator + string.Join(dirSeparator, relativePaths);
			if (!path.EndsWith(dirSeparator))
				path += dirSeparator;
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
