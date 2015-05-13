using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teltec.Backup.Data.Models;
using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Versioning
{
	public sealed class DefaultRestoreScanner : PathScanner<CustomVersionedFile>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationToken CancellationToken;
		RestorePlan Plan;
		LinkedList<CustomVersionedFile> Result;

		public DefaultRestoreScanner(RestorePlan plan, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Plan = plan;
		}

		#region PathScanner

		// TODO(jweyrich): Should return a HashSet/ISet instead?
		public override LinkedList<CustomVersionedFile> Scan()
		{
			Result = new LinkedList<CustomVersionedFile>();

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
								AddDirectory(entry);
								break;
							}
						case EntryType.FOLDER:
							{
								AddDirectory(entry);
								break;
							}
						case EntryType.FILE:
							{
								AddFile(entry);
								break;
							}
						case EntryType.FILE_VERSION:
							{
								AddFileVersion(entry);
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
					string message = string.Format("Failed to scan entry \"{0}\" - {1}", entry.Path, ex.Message);
					logger.Error(message, ex);
				}
			}

			return Result;
		}

		#endregion

		private void AddDirectory(BackupPlanPathNode node, IFileVersion version)
		{
			CancellationToken.ThrowIfCancellationRequested();

			// Add all files from this directory.
			foreach (BackupPlanPathNode subNode in node.SubNodes)
			{
				if (subNode.Type != EntryType.FILE)
					continue;

				AddFile(subNode, version);
			}

			// Add all sub-directories recursively.
			foreach (BackupPlanPathNode subNode in node.SubNodes)
			{
				if (subNode.Type != EntryType.FOLDER)
					continue;

				AddDirectory(subNode, version);
			}
		}

		private void AddFileVersion(BackupPlanPathNode node, IFileVersion version)
		{
			CancellationToken.ThrowIfCancellationRequested();

			long size = 0;

			// If `version` is not not informed, then find the file's latest version.
			if (version == null)
			{
				BackupedFile f = node.PlanFile.Versions.Last();
				IFileVersion latestFileVersion = f != null
					? new FileVersion { Name = f.Backup.VersionName, Version = f.Backup.Version }
					: null;
				version = latestFileVersion;
				size = f.FileSize;
			}
			else
			{
				BackupedFile f = node.PlanFile.Versions.First(p => p.Backup.Version.Equals(version.Version));
				size = f.FileSize;
			}

			var item = new CustomVersionedFile { Path = node.Path, Version = version, Size = size };

			Result.AddLast(item);

			if (FileAdded != null)
				FileAdded(this, item);
		}

		private void AddFile(BackupPlanPathNode node, IFileVersion version)
		{
			AddFileVersion(node, version);
		}

		private void AddFileVersion(RestorePlanSourceEntry entry)
		{
			IFileVersion version = entry.Version == null ? null : new FileVersion { Version = entry.Version };
			AddFile(entry.PathNode, version);
		}

		private void AddFile(RestorePlanSourceEntry entry)
		{
			IFileVersion version = entry.Version == null ? null : new FileVersion { Version = entry.Version };
			AddFile(entry.PathNode, version);
		}

		private void AddDirectory(RestorePlanSourceEntry entry)
		{
			IFileVersion version = entry.Version == null ? null : new FileVersion { Version = entry.Version };
			AddDirectory(entry.PathNode, version);
		}
	}
}
