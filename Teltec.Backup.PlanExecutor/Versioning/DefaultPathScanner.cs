using NLog;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Teltec.Backup.Data.Models;
using Teltec.Stats;
using Teltec.Storage;

namespace Teltec.Backup.PlanExecutor.Versioning
{
	public sealed class DefaultPathScanner : PathScanner<string>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationToken CancellationToken;
		BackupPlan Plan;

		public DefaultPathScanner(BackupPlan plan, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Plan = plan;
		}

		#region PathScanner

		// TODO(jweyrich): Should return a HashSet/ISet instead?
		[MethodImpl(MethodImplOptions.NoInlining)]
		public override void Scan()
		{
			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			Results = new PathScanResults<string>();

			//
			// Add sources.
			//
			foreach (var entry in Plan.SelectedSources)
			{
				try
				{
					switch (entry.Type)
					{
						default:
							throw new InvalidOperationException("Unhandled EntryType");
						case EntryType.DRIVE:
							{
								var dir = new ZetaLongPaths.ZlpDirectoryInfo(new DriveInfo(entry.Path).RootDirectory.FullName);
								AddDirectory(dir);
								break;
							}
						case EntryType.FOLDER:
							{
								var dir = new ZetaLongPaths.ZlpDirectoryInfo(entry.Path);
								AddDirectory(dir);
								break;
							}
						case EntryType.FILE:
							{
								var file = new ZetaLongPaths.ZlpFileInfo(entry.Path);
								AddFile(file);
								break;
							}
					}
				}
				catch (OperationCanceledException)
				{
					throw ex; // Rethrow!
				}
				catch (Exception ex)
				{
					HandleException(entry.Type, entry.Path, ex);
				}
			}

			stats.End();
		}

		#endregion

		private void AddFile(ZetaLongPaths.ZlpFileInfo file)
		{
			CancellationToken.ThrowIfCancellationRequested();

			try
			{
				// IMPORTANT: The condition below, `if (!file.Exists)`, doesn't guarantee the
				// file will exist after this statement! We probably shouldn't try to detect this but
				// handle any exceptions that may be raised.
				if (!file.Exists)
				{
					// FileInfo.FullName may throw:
					//    System.IO.PathTooLongException
					//    System.Security.SecurityException
					logger.Warn("File {0} does not exist", file.FullName);
					return;
				}

				var item = file.FullName;

				// IMPORTANT: Strip the \\?\ prefix Windows uses for long paths (those > MAX_PATH).
				if (item != null && item.StartsWith(@"\\?\"))
					item = item.Substring(4);

				Results.AddedFile(item);

				if (FileAdded != null)
					FileAdded(this, item);
			}
			catch (OperationCanceledException)
			{
				throw ex; // Rethrow!
			}
			catch (Exception ex)
			{
				HandleException(EntryType.FILE, file.FullName, ex);
			}
		}

		private void AddDirectory(string path)
		{
			AddDirectory(new ZetaLongPaths.ZlpDirectoryInfo(path));
		}

		private void AddDirectory(ZetaLongPaths.ZlpDirectoryInfo directory)
		{
			CancellationToken.ThrowIfCancellationRequested();

			try
			{
				// IMPORTANT: The condition below, `if (!directory.Exists)`, doesn't guarantee the
				// directory will exist after this statement! We probably shouldn't try to detect this but
				// handle any exceptions that may be raised.
				if (!directory.Exists)
				{
					// DirectoryInfo.FullName may throw:
					//    System.IO.PathTooLongException
					//    System.Security.SecurityException
					logger.Warn("Directory {0} does not exist", directory.FullName);
					return;
				}

				ZetaLongPaths.ZlpFileInfo[] files = directory.GetFiles(); // System.IO.DirectoryNotFoundException
				// Add all files from this directory.
				foreach (var file in files)
					AddFile(file);

				ZetaLongPaths.ZlpDirectoryInfo[] directories = directory.GetDirectories();
				// Add all sub-directories recursively.
				foreach (var subdir in directories)
					AddDirectory(subdir);
			}
			catch (OperationCanceledException)
			{
				throw ex; // Rethrow!
			}
			catch (Exception ex)
			{
				HandleException(EntryType.FOLDER, directory.FullName, ex);
			}
		}

		private void HandleException(EntryType type, string path, Exception ex)
		{
			string message = null;

			switch (type)
			{
				case EntryType.DRIVE:
					message = string.Format("Failed to scan volume \"{0}\" - {1}", path, ex.Message);
					break;
				case EntryType.FOLDER:
					message = string.Format("Failed to scan directory \"{0}\" - {1}", path, ex.Message);
					break;
				case EntryType.FILE:
					message = string.Format("Failed to scan file \"{0}\" - {1}", path, ex.Message);
					break;
			}

			if (EntryScanFailed != null)
				EntryScanFailed(this, path, message, ex);

			if (!string.IsNullOrEmpty(message))
			{
				logger.Log(LogLevel.Error, ex, message);

				// TODO: Should we register this to show ERRORS/FAILURES in the backup operation?
				Results.FailedFile(path, message);
			}
		}
	}
}
