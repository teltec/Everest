/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teltec.FileSystem
{
	public class FileManager
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static DateTime UnsafeGetFileLastWriteTimeUtc(string path)
		{
			//return File.GetLastWriteTimeUtc(path);
			return ZetaLongPaths.ZlpIOHelper.GetFileLastWriteTime(path).ToUniversalTime();
		}

		public static DateTime? SafeGetFileLastWriteTimeUtc(string path)
		{
			try
			{
				return UnsafeGetFileLastWriteTimeUtc(path);
			}
			catch (Exception e)
			{
				logger.Error("Failed to get LastWriteTime of file/directory \"{0}\" - {1}", path, e.Message);
				return null;
			}
		}

		public static void UnsafeSetFileLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			//File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
			ZetaLongPaths.ZlpIOHelper.SetFileLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
		}

		public static void SafeSetFileLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			try
			{
				UnsafeSetFileLastWriteTimeUtc(path, lastWriteTimeUtc);
			}
			catch (Exception e)
			{
				logger.Error("Failed to change LastWriteTime of file/directory \"{0}\" - {1}", path, e.Message);
			}
		}

		public static long UnsafeGetFileSize(string path)
		{
			//return new FileInfo(path).Length;
			return ZetaLongPaths.ZlpIOHelper.GetFileLength(path);
		}

		public static long? SafeGetFileSize(string path)
		{
			try
			{
				return UnsafeGetFileSize(path);
			}
			catch (Exception e)
			{
				logger.Error("Failed to get file size of file/directory \"{0}\" - {1}", path, e.Message);
				return null;
			}
		}

		public static string UnsafeGetDirectoryName(string filePath)
		{
			ZetaLongPaths.ZlpFileInfo file = new ZetaLongPaths.ZlpFileInfo(filePath);
			return file.DirectoryName;
		}

		public static string SafeGetDirectoryName(string filePath)
		{
			try
			{
				return UnsafeGetDirectoryName(filePath);
			}
			catch (Exception e)
			{
				logger.Error("Failed to get directory name for file \"{0}\" - {1}", filePath, e.Message);
				return null;
			}
		}

		public static void UnsafeCreateDirectory(string path)
		{
			//DirectoryInfo info = Directory.CreateDirectory(path);
			ZetaLongPaths.ZlpIOHelper.CreateDirectory(path);
		}

		public static bool SafeCreateDirectory(string path)
		{
			try
			{
				UnsafeCreateDirectory(path);
				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to create directory \"{0}\" - {1}", path, e.Message);
				return false;
			}
		}

		public static bool DeleteDirectory(string path, bool recursive = false)
		{
			//if (!Directory.Exists(path))
			if (!ZetaLongPaths.ZlpIOHelper.DirectoryExists(path))
			{
				logger.Warn("Directory does not exist \"{0}\"", path);
				return false;
			}

			try
			{
				//Directory.Delete(path, recursive);
				ZetaLongPaths.ZlpIOHelper.DeleteDirectory(path, recursive);
				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to delete directory \"{0}\" - {1}", path, e.Message);
				return false;
			}
		}

		public static bool DeleteFile(string path)
		{
			//if (!File.Exists(path))
			if (!ZetaLongPaths.ZlpIOHelper.FileExists(path))
			{
				logger.Warn("File does not exist \"{0}\"", path);
				return false;
			}

			try
			{
				//File.Delete(path);
				ZetaLongPaths.ZlpIOHelper.DeleteFile(path);
				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to delete file \"{0}\" - {1}", path, e.Message);
				return false;
			}
		}

		public static bool CopyFile(string sourcePath, string targetPath, bool overwrite = false)
		{
			//if (!File.Exists(sourcePath))
			if (!ZetaLongPaths.ZlpIOHelper.FileExists(sourcePath))
			{
				logger.Warn("Source file does not exist \"{0}\"", sourcePath);
				return false;
			}

			try
			{
				//File.Copy(sourcePath, targetPath, overwrite);
				ZetaLongPaths.ZlpIOHelper.CopyFileExact(sourcePath, targetPath, overwrite);
				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to copy file \"{0}\" to \"{1}\" - {2}", sourcePath, targetPath, e.Message);
				return false;
			}
		}

		public static bool FileExists(string path)
		{
			return ZetaLongPaths.ZlpIOHelper.FileExists(path);
		}

		public static bool VolumeExists(string letter)
		{
			if (string.IsNullOrEmpty(letter))
				return false;

			letter = letter.ToUpper();
			return GetVolumes().Contains(letter[0]);
		}

		public static string GetDriveLetter(string path)
		{
			return ZetaLongPaths.ZlpPathHelper.GetDrive(path); // Transforms @"C:\Foo\Bar" into @"C:"
		}

		private static char[] GetVolumes()
		{
			List<char> result = new List<char>();
			foreach (string drive in Directory.GetLogicalDrives())
			{
				string letter = drive.ToUpper();
				result.Add(letter[0]);
			}
			return result.ToArray();
		}
	}
}
