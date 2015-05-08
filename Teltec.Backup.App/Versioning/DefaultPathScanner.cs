using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teltec.Backup.App.Models;
using Teltec.Storage;

namespace Teltec.Backup.App.Versioning
{
	public sealed class DefaultPathScanner : PathScanner<string>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationToken CancellationToken;
		BackupPlan Plan;
		LinkedList<string> Result;

		public DefaultPathScanner(BackupPlan plan, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Plan = plan;
		}

		#region PathScanner

		// TODO(jweyrich): Should return a HashSet/ISet instead?
		public override LinkedList<string> Scan()
		{
			Result = new LinkedList<string>();

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
								DirectoryInfo dir = new DriveInfo(entry.Path).RootDirectory;
								AddDirectory(dir);
								break;
							}
						case EntryType.FOLDER:
							{
								DirectoryInfo dir = new DirectoryInfo(entry.Path);
								AddDirectory(dir);
								break;
							}
						case EntryType.FILE:
							{
								FileInfo file = new FileInfo(entry.Path);
								AddFile(file);
								break;
							}
					}
				}
				catch (OperationCanceledException ex)
				{
					throw ex; // Rethrow!
				}
				catch (Exception ex)
				{
					string message = string.Format("Failed to scan entry {0}: {1}", entry.Path, ex.Message);
					logger.Error(message, ex);
				}
			}

			return Result;
		}

		#endregion

		private void AddFile(FileInfo file)
		{
			if (!file.Exists)
			{
				logger.Warn("File {0} does not exist", file.FullName);
				return;
			}

			// IMPORTANT: The condition above, `if (!directory.Exists)`, doesn't guarantee the
			// directory will exist at this point! We probably shouldn't try to detect this but
			// handle any exceptions that may be raised.

			CancellationToken.ThrowIfCancellationRequested();

			var item = file.FullName;

			Result.AddLast(item);

			if (FileAdded != null)
				FileAdded(this, item);
		}

		private void AddDirectory(string path)
		{
			AddDirectory(new DirectoryInfo(path));
		}

		private void AddDirectory(DirectoryInfo directory)
		{
			if (!directory.Exists)
			{
				logger.Warn("Directory {0} does not exist", directory.FullName);
				return;
			}

			// IMPORTANT: The condition above, `if (!directory.Exists)`, doesn't guarantee the
			// directory will exist at this point! We probably shouldn't try to detect this but
			// handle any exceptions that may be raised.

			CancellationToken.ThrowIfCancellationRequested();

			FileInfo[] files = directory.GetFiles(); // System.IO.DirectoryNotFoundException
			// Add all files from this directory.
			foreach (FileInfo file in files)
				AddFile(file);

			DirectoryInfo[] directories = directory.GetDirectories();
			// Add all sub-directories recursively.
			foreach (DirectoryInfo subdir in directories)
				AddDirectory(subdir);
		}
	}
}
