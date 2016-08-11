using NLog;
using System;
using System.Diagnostics;

namespace Teltec.Common.Utils
{
	public static class ProcessUtils
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static Process StartSubProcess(string filename, string arguments, string cwd, EventHandler onExit = null)
		{
			//
			// CITATIONS:
			//
			//   The LocalSystem account is a predefined local account used by the service control manager.
			//   It has extensive privileges on the local computer, and acts as the computer on the network.
			//
			//   - The registry key HKEY_CURRENT_USER is associated with the default user, not the current user.
			//     To access another user's profile, impersonate the user, then access HKEY_CURRENT_USER.
			//   - The service presents the computer's credentials to remote servers.
			//
			// REFERENCE: https://msdn.microsoft.com/en-us/library/ms684190(VS.85).aspx
			//
			try
			{
				ProcessStartInfo info = new ProcessStartInfo(filename, arguments);
				info.WorkingDirectory = cwd;
#if DEBUG
				info.CreateNoWindow = true;
#else
				info.CreateNoWindow = false;
#endif
				Process process = new Process();
				process.StartInfo = info;
				process.EnableRaisingEvents = true;
				if (onExit != null)
					process.Exited += onExit;
				logger.Info("Starting sub-process {0} {1}", filename, arguments);
				process.Start();
				return process;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, ex, "Failed to start sub-process {0} {1}", filename, arguments);
				throw;
			}
		}
	}
}
