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

		public static DateTime UnsafeGetLastWriteTimeUtc(string path)
		{
			//return File.GetLastWriteTimeUtc(path);
			return ZetaLongPaths.ZlpIOHelper.GetFileLastWriteTime(path).ToUniversalTime();
		}

		public static DateTime? SafeGetLastWriteTimeUtc(string path)
		{
			try
			{
				return UnsafeGetLastWriteTimeUtc(path);
			}
			catch (Exception e)
			{
				logger.Error("Failed to get LastWriteTime of file/directory \"{0}\" - {1}", path, e.Message);
				return null;
			}
		}

		public static void UnsafeSetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			//File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
			ZetaLongPaths.ZlpIOHelper.SetFileLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
		}

		public static void SafeSetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			try
			{
				UnsafeSetLastWriteTimeUtc(path, lastWriteTimeUtc);
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

		public static bool CreateDirectory(string path)
		{
			try
			{
				//DirectoryInfo info = Directory.CreateDirectory(path);
				ZetaLongPaths.ZlpIOHelper.CreateDirectory(path);
				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to create directory \"{0}\" - {1}", path, e.Message);
				return false;
			}
		}

		public static bool VolumeExists(string letter)
		{
			if (string.IsNullOrEmpty(letter))
				return false;

			letter = letter.ToUpper();
			return GetVolumes().Contains(letter[0]);
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
