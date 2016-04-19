using System;
using System.Collections.Generic;
using System.IO;
using Teltec.FileSystem;

namespace Teltec.Backup.App.Controls
{
	public class DriveItemsEnumerable : IEnumerable<DriveItem>
	{
		public IEnumerator<DriveItem> GetEnumerator()
		{
			return FetchDriveList();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool _ExcludeRemovableDrives = false;
		public bool ExcludeRemovableDrives
		{
			get { return _ExcludeRemovableDrives; }
			set { _ExcludeRemovableDrives = value; }
		}

		private bool _ExcludeFixedDrives = false;
		public bool ExcludeFixedDrives
		{
			get { return _ExcludeFixedDrives; }
			set { _ExcludeFixedDrives = value; }
		}

		private bool _ExcludeCDRomDrives = false;
		public bool ExcludeCDRomDrives
		{
			get { return _ExcludeCDRomDrives; }
			set { _ExcludeCDRomDrives = value; }
		}

		private bool _ExcludeNetworkDrives = false;
		public bool ExcludeNetworkDrives
		{
			get { return _ExcludeNetworkDrives; }
			set { _ExcludeNetworkDrives = value; }
		}

		private static readonly string AllDrives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private IEnumerator<DriveItem> FetchDriveList()
		{
			yield return new DriveItem { Text = "Please, select a drive" };

			DriveInfo[] allDrivesInUse = DriveInfo.GetDrives();
			foreach (char drive in AllDrives)
			{
				string localDriveFullName = drive.ToString() + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar; // For example, @"C:\".

				DriveInfo driveInUse = Array.Find(allDrivesInUse, x => x.RootDirectory.FullName == localDriveFullName);

				if (driveInUse != null)
				{
					DriveItem item = null;
					string localDriveName = FileManager.GetDriveLetter(driveInUse.RootDirectory.FullName); // For example: @"C:" (without trailing slash)
					string driveType = driveInUse.DriveType.ToString();

					switch (driveInUse.DriveType)
					{
						default:
							item = new DriveItem
							{
								Text = string.Format("{0} ({1})", localDriveName, driveType),
								LocalDrive = localDriveName,
								IsDriveAvailable = false,
							};
							break;
						case DriveType.Network:
							if (ExcludeNetworkDrives)
								break;
							string mappedPath = MappedDriveResolver.ResolveToRootUNC(driveInUse.RootDirectory.FullName);
							item = new DriveItem
							{
								Text = string.Format("{0} ({1})", localDriveName, mappedPath),
								LocalDrive = localDriveName,
								MappedPath = mappedPath,
								IsDriveAvailable = false,
							};
							break;
						case DriveType.Fixed:
							if (ExcludeFixedDrives)
								break;
							item = new DriveItem
							{
								Text = string.Format("{0} ({1})", localDriveName, driveType),
								LocalDrive = localDriveName,
								IsDriveAvailable = false,
							};
							break;
						case DriveType.Removable:
							if (ExcludeRemovableDrives)
								break;
							item = new DriveItem
							{
								Text = string.Format("{0} ({1})", localDriveName, driveType),
								LocalDrive = localDriveName,
								IsDriveAvailable = false,
							};
							break;
						case DriveType.CDRom:
							if (ExcludeCDRomDrives)
								break;
							item = new DriveItem
							{
								Text = string.Format("{0} ({1})", localDriveName, driveType),
								LocalDrive = localDriveName,
								IsDriveAvailable = false,
							};
							break;
					}

					if (item != null)
						yield return item;
				}
				else
				{
					string localDriveName = FileManager.GetDriveLetter(localDriveFullName); // For example: @"C:" (without trailing slash)
					yield return new DriveItem
					{
						Text = localDriveName,
						LocalDrive = localDriveName,
						IsDriveAvailable = true,
					};
				}
			}
		}
	}
}
