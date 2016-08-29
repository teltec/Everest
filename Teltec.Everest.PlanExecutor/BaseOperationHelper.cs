using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Teltec.Everest.Data.DAO;
using Teltec.FileSystem;
using Teltec.Storage;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor
{
	public class BaseOperationHelper
	{
		public class FailedToMountNetworkDrive : Exception
		{
			public FailedToMountNetworkDrive(string message)
				: base(message)
			{
			}
		}

		public class FailedToExecuteUserDefinedAction : Exception
		{
			public FailedToExecuteUserDefinedAction(string message)
				: base(message)
			{
			}
		}

		static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly Models.ISchedulablePlan Plan;

		public BaseOperationHelper(Models.ISchedulablePlan plan)
		{
			Plan = plan;
			Plan.Config.WireUpActions(); // IMPORTANT: Must be invoked before raising any plan event.
		}

		#region Network drive mapping

		private DriveInfo GetDriveInfo(string drive)
		{
			DriveInfo[] drivesInUse = DriveInfo.GetDrives();
			foreach (DriveInfo d in drivesInUse)
			{
				if (d.RootDirectory.FullName.StartsWith(drive))
					return d;
			}
			return null;
		}

		private void MountNetworkDrive(Models.NetworkCredential cred)
		{
			string message = null;
			try
			{
				DriveInfo drive = GetDriveInfo(cred.MountPoint);
				if (drive != null)
				{
					string userName = null;
					string remotePath = MappedDriveResolver.ResolveToRootUNC(cred.MountPoint);
					string credentialUsed = MappedDriveResolver.GetCredentialUsedToMapNetworkDrive(cred.MountPoint);
					message = string.Format("{0} is already mounted to {1} by {2}", cred.MountPoint, remotePath, credentialUsed);
					logger.Info(message);

					// Was this mounted using the same credential?
					// We assume the mount point is OK if we cannot retrieve the username that originally mounted it.
					if (userName != null && (userName.Equals(cred.Login, StringComparison.InvariantCulture)
						|| userName.Equals(MappedDriveResolver.UNKNOWN_CREDENTIAL, StringComparison.InvariantCulture)))
					{
						return; // We're OK then.
					}

					NetworkDriveMapper.UnmountNetworkLocation(cred.MountPoint);
					logger.Info("Umounted {0}", cred.MountPoint);
				}

				NetworkDriveMapper.MountNetworkLocation(cred.MountPoint, cred.Path, cred.Login, cred.Password, false);
				message = string.Format("Successfully mounted {0} to {1} as {2}", cred.MountPoint, cred.Path, cred.Login);
				logger.Info(message);
			}
			catch (Win32Exception ex)
			{
				string reason = ex.Message;

				switch (ex.NativeErrorCode)
				{
					case NetworkDriveMapper.ERROR_ALREADY_ASSIGNED:
						{
							//string userName = null;
							string remotePath = MappedDriveResolver.ResolveToRootUNC(cred.MountPoint);
							string credentialUsed = MappedDriveResolver.GetCredentialUsedToMapNetworkDrive(cred.MountPoint);
							reason = string.Format("It's already mounted to {0} by {1}", remotePath, credentialUsed);
							break;
						}
				}

				message = string.Format("Failed to mount {0} to {1} as {2} - {3}", cred.MountPoint, cred.Path, cred.Login, reason);
				throw new FailedToMountNetworkDrive(message);
			}
		}

		public void MountAllNetworkDrives()
		{
			NetworkCredentialRepository dao = new NetworkCredentialRepository();
			List<Models.NetworkCredential> allCredentials = dao.GetAll();

			if (allCredentials.Count == 0)
				return;

			logger.Info("Mounting network shares...");

			foreach (Models.NetworkCredential cred in allCredentials)
			{
				MountNetworkDrive(cred);
			}
		}

		#endregion

		#region Pre-actions

		public void ExecutePreActions()
		{
			try
			{
				bool actionSuccess = Plan.Config.OnBeforePlanStarts(new Models.PlanEventArgs { Plan = Plan });
				if (!actionSuccess)
				{
					string message = "Pre-action did not succeed.";
					throw new FailedToExecuteUserDefinedAction(message);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Post-actions

		public void ExecutePostActions(TransferResults xferResults)
		{
			try
			{
				bool actionSuccess = Plan.Config.OnAfterPlanFinishes(new Models.PlanEventArgs { Plan = Plan, OperationResult = xferResults.OverallStatus });
				if (!actionSuccess)
				{
					string message = "Post-action did not succeed.";
					throw new FailedToExecuteUserDefinedAction(message);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion
	}
}
