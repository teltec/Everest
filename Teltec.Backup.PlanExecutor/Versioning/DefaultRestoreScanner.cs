using NLog;
using System;
using System.Linq;
using System.Threading;
using Teltec.Backup.Data.Models;
using Teltec.Backup.Data.Versioning;
using Teltec.Stats;
using Teltec.Storage;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.PlanExecutor.Versioning
{
	public sealed class DefaultRestoreScanner : PathScanner<CustomVersionedFile>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationToken CancellationToken;
		RestorePlan Plan;

		public DefaultRestoreScanner(RestorePlan plan, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Plan = plan;
		}

		#region PathScanner

		// TODO(jweyrich): Should return a HashSet/ISet instead?
		public override void Scan()
		{
			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			Results = new PathScanResults<CustomVersionedFile>();

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
				catch (OperationCanceledException)
				{
					throw ex; // Rethrow!
				}
				catch (Exception ex)
				{
					string message = string.Format("Failed to scan entry \"{0}\" - {1}", entry.Path, ex.Message);
					logger.Log(LogLevel.Error, ex, message);
				}
			}

			stats.End();
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

			BackupedFile f = null;

			// If `version` is not not informed, then find the file's latest version
			// that has completed transfer.
			if (version == null)
			{
				f = node.PlanFile.Versions.LastOrDefault(v => v.TransferStatus == TransferStatus.COMPLETED);

				IFileVersion latestFileVersion = f != null && f.Id.HasValue
					? new FileVersion { Name = f.VersionName, Version = f.Version }
					: null;
				version = latestFileVersion;
			}
			else
			{
				f = node.PlanFile.Versions.First(
					p => p.Version.Equals(version.Version, StringComparison.InvariantCulture));
			}

			if (f == null || !f.Id.HasValue)
				return;

			var item = new CustomVersionedFile
			{
				Path = node.Path,
				Size = f.FileSize,
				LastWriteTimeUtc = f.FileLastWrittenAt,
				UserData = f, // Reference the original `BackupedFile`.
				Version = version,
			};

			Results.Files.AddLast(item);

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
