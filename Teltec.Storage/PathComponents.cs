using System;
using System.Collections.Generic;
using System.IO;

namespace Teltec.Storage
{
	public class PathComponents
	{
		[Flags]
		public enum ComponentFlags
		{
			NONE = 0,
			DRIVE = 1,
			DIRECTORY = 2,
			FILENAME = 4,
			EXTENSION = 8,
		}

		public ComponentFlags AvailableComponents { get; private set; }
		public string FullPath { get; private set; }
		public string Drive { get; private set; }
		public string[] Directories { get; private set; }
		public string FileName { get; private set; }
		public string FileNameWithoutExtension { get; private set; }
		public string Extension { get; private set; } // Without the leading period '.'

		public PathComponents(string path)
		{
			Parse(path);
		}

		public bool HasDrive { get { return AvailableComponents.HasFlag(ComponentFlags.DRIVE); } }
		public bool HasDirectories { get { return AvailableComponents.HasFlag(ComponentFlags.DIRECTORY); } }
		public bool HasFileName { get { return AvailableComponents.HasFlag(ComponentFlags.FILENAME); } }
		public bool HasExtension { get { return AvailableComponents.HasFlag(ComponentFlags.EXTENSION); } }

		private void Reset()
		{
			AvailableComponents = ComponentFlags.NONE;
			FullPath = null;
			Drive = null;
			Directories = null;
			FileName = null;
			FileNameWithoutExtension = null;
			Extension = null;
		}

		public ComponentFlags Parse(string path, bool resolve = false)
		{
			Reset();

			ComponentFlags comps = ComponentFlags.NONE;

			// Full path
			string fullpath = resolve
				? Path.GetFullPath(path) // Will throw an exception for an invalid path string.
				: path;

			// Drive
			string driveUnmodified = Path.GetPathRoot(fullpath);
			string drive = driveUnmodified;
			// Remove root from Windows path.
			if (drive != null && (drive.EndsWith(":") || drive.EndsWith(@":\"))) // ?? Path.VolumeSeparatorChar.ToString() + Path.DirectorySeparatorChar
				drive = drive[0].ToString();
			//// Remove root from Linux/Unix path, if any.
			//if (drive != null && drive.StartsWith("/", StringComparison.Ordinal))
			//	drive = drive.Substring(1);
			if (!string.IsNullOrEmpty(drive))
				comps |= ComponentFlags.DRIVE;

			// Directories
			string[] directories = null;
			string parsing = fullpath;
			int whereItBegins = driveUnmodified.Length;
			if (whereItBegins != -1)
				parsing = parsing.Substring(whereItBegins);
			int whereItEnds = parsing.LastIndexOf(Path.DirectorySeparatorChar);
			if (whereItEnds != -1)
			{
				parsing = parsing.Remove(whereItEnds);
				if (parsing.Length > 0)
				{
					directories = parsing.Split(Path.DirectorySeparatorChar);
					if (directories.Length > 0)
						comps |= ComponentFlags.DIRECTORY;
				}
			}
			//Console.WriteLine("whereItBegins = {0}", whereItBegins);
			//Console.WriteLine("whereItEnds = {0}", whereItEnds);
			//Console.WriteLine("parsing = {0}", parsing);
			//Console.WriteLine("directories = {0}", directories);

			// File name, with and without extension
			string filename = Path.GetFileName(fullpath);
			string filenameWithoutExtension = string.Empty;
			if (!string.IsNullOrEmpty(filename))
			{
				filenameWithoutExtension = Path.GetFileNameWithoutExtension(fullpath);
				comps |= ComponentFlags.FILENAME;
			}

			// File extension
			string extension = Path.GetExtension(fullpath);
			if (!string.IsNullOrEmpty(extension))
			{
				extension = extension.Substring(1); // Remove the leading period '.'
				comps |= ComponentFlags.EXTENSION;
			}

			// Ta-da!
			AvailableComponents = comps;
			FullPath = fullpath;
			Drive = drive;
			Directories = directories;
			FileName = filename;
			FileNameWithoutExtension = filenameWithoutExtension;
			Extension = extension;

			return AvailableComponents;
		}

		public string Combine()
		{
			string result = "";

			if (HasDrive)
				result += Drive + Path.VolumeSeparatorChar
					+ Path.DirectorySeparatorChar;

			if (HasDirectories)
				result += string.Join(Path.DirectorySeparatorChar.ToString(), Directories)
					+ Path.DirectorySeparatorChar;

			if (HasFileName)
				result += FileName;

			if (HasExtension)
				result += '.' + Extension;

			return null;
		}
	}
}
