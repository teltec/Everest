﻿using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teltec.Backup.App
{
	public class FileManager
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static DateTime? GetLastWriteTimeUtc(string path)
		{
			try
			{
				return File.GetLastWriteTimeUtc(path);
			}
			catch (Exception e)
			{
				logger.Error("Failed to read file/directory \"{0}\" - {1}", path, e.Message);
				return null;
			}
		}

		public static long? GetFileSize(string path)
		{
			try
			{
				return new FileInfo(path).Length;
			}
			catch (Exception e)
			{
				logger.Error("Failed to read file/directory \"{0}\" - {1}", path, e.Message);
				return null;
			}
		}

		public static bool DeleteDirectory(string path, bool recursive = false)
		{
			if (!Directory.Exists(path))
			{
				logger.Warn("Directory does not exist \"{0}\"", path);
				return false;
			}

			try
			{
				Directory.Delete(path, recursive);
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
			if (!File.Exists(path))
			{
				logger.Warn("File does not exist \"{0}\"", path);
				return false;
			}

			try
			{
				File.Delete(path);
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
			if (!File.Exists(sourcePath))
			{
				logger.Warn("Source file does not exist \"{0}\"", sourcePath);
				return false;
			}

			try
			{
				File.Copy(sourcePath, targetPath, overwrite);
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
				DirectoryInfo info = Directory.CreateDirectory(path);
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