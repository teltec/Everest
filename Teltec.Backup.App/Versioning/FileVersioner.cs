using App;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.App.Models;
using Teltec.Storage;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Versioning
{
	public class FileVersioner
	{
		CancellationTokenSource CancellationTokenSource;
		Models.BackupPlan Plan;

		public FileVersioner(Models.BackupPlan plan)
		{
			CancellationTokenSource = new CancellationTokenSource();
			Plan = plan;
		}

		public async Task AssembleVersion(BackupAgent agent)
		{
			await ExecuteOnBackround(() =>
			{
				ProcessSources(agent);
			}, CancellationTokenSource.Token);
		}

		static int version = 0;

		protected void ProcessSources(BackupAgent agent)
		{
			version++;

			//
			// Add sources.
			//
			foreach (var entry in Plan.SelectedSources)
			{
				switch (entry.Type)
				{
					case BackupPlanSourceEntry.EntryType.DRIVE:
						{
							DirectoryInfo dir = new DriveInfo(entry.Path).RootDirectory;
							agent.AddDirectory(dir);
							break;
						}
					case BackupPlanSourceEntry.EntryType.FOLDER:
						{
							DirectoryInfo dir = new DirectoryInfo(entry.Path);
							agent.AddDirectory(dir);
							break;
						}
					case BackupPlanSourceEntry.EntryType.FILE:
						{
							FileInfo file = new FileInfo(entry.Path);
							VersionedFileInfo versionedFile = new VersionedFileInfo(file, new FileVersion { Version = version.ToString() });
							agent.AddFile(versionedFile);
							break;
						}
				}
			}
		}

		public void Cancel()
		{
			InternalCancel();
		}

		protected void InternalCancel()
		{
			CancellationTokenSource.Cancel();
		}

		protected Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return AsyncHelper.ExecuteOnBackround(action, token);
		}
	}
}
