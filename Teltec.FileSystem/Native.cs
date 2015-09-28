using System;

namespace Teltec.FileSystem
{
	public static class Native
	{
		#region System detection

		private static bool _DidDetectSystem = false;
		private static bool _IsRunningOnWindows = false;
		private static bool _IsRunningOnUnix = false;
		private static bool _IsRunningOnMac = false;

		public static bool IsRunningOnWindows { get { DetectSystem(); return _IsRunningOnWindows; } }
		public static bool IsRunningOnUnix { get { DetectSystem(); return _IsRunningOnUnix; } }
		public static bool IsRunningOnMac { get { DetectSystem(); return _IsRunningOnMac; } }

		public static void DetectSystem()
		{
			if (_DidDetectSystem)
				return;
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
			switch (pid)
			{
				default: throw new SystemException("Couldn't detect which system I'm running on.");
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					_IsRunningOnWindows = true;
					break;
				case PlatformID.Unix:
					_IsRunningOnUnix = true;
					break;
				case PlatformID.MacOSX:
					_IsRunningOnUnix = true;
					_IsRunningOnMac = true;
					break;
			}
			_DidDetectSystem = true;
		}

		#endregion
	}
}
