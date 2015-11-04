using NLog;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Teltec.FileSystem
{
	public class NetworkDriveMapper
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void MountNetworkLocation(string localDriveName, string remotePath, string username, string password, bool promptUser)
		{
			localDriveName = FileManager.GetDriveLetter(localDriveName);

			var resource = new NETRESOURCE
			{
				dwType = ResourceType.DISK,
				lpLocalName = localDriveName,
				lpRemoteName = remotePath,
				lpProvider = null
			};

			int ret = WNetUseConnection(
				IntPtr.Zero, resource,
				promptUser ? null : password,
				promptUser ? null : username,
				promptUser ? Connect.INTERACTIVE | Connect.PROMPT : 0,
				null, null, null);

			if (ret != NO_ERROR)
				throw new Win32Exception(ret);
		}

		public static void UnmountNetworkLocation(string localDriveNameOrRemotePath)
		{
			int ret = WNetCancelConnection2(localDriveNameOrRemotePath, Connect.UPDATE_PROFILE, false);
			if (ret != NO_ERROR)
				throw new Win32Exception(ret);
		}

		#region Network PInvoke

		public const int NO_ERROR = 0;
		public const int ERROR_ALREADY_ASSIGNED = 85;

		string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		[DllImport("Mpr.dll")]
		private static extern int WNetUseConnection
		(
			IntPtr hwndOwner,
			NETRESOURCE lpNetResource,
			string lpPassword,
			string lpUserID,
			Connect dwFlags,
			string lpAccessName,
			string lpBufferSize,
			string lpResult
		);

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.wnetcancelconnection2
		[DllImport("mpr.dll")]
		static extern int WNetCancelConnection2(string lpName, Connect dwFlags, bool bForce);

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		public enum ResourceScope
		{
			CONNECTED = 0x00000001,
			GLOBALNET = 0x00000002,
			REMEMBERED = 0x00000003,
		}

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		public enum ResourceType
		{
			ANY = 0x00000000,
			DISK = 0x00000001,
			PRINT = 0x00000002,
		}

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		public enum ResourceDisplayType
		{
			GENERIC = 0x00000000,
			DOMAIN = 0x00000001,
			SERVER = 0x00000002,
			SHARE = 0x00000003,
			FILE = 0x00000004,
			GROUP = 0x00000005,
			NETWORK = 0x00000006,
			ROOT = 0x00000007,
			SHAREADMIN = 0x00000008,
			DIRECTORY = 0x00000009,
			TREE = 0x0000000A,
			NDSCONTAINER = 0x0000000A,
		}

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		[Flags]
		public enum ResourceUsage
		{
			CONNECTABLE = 0x00000001,
			CONTAINER = 0x00000002,
			NOLOCALDEVICE = 0x00000004,
			SIBLING = 0x00000008,
			ATTACHED = 0x00000010,
		}

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		[Flags]
		public enum Connect
		{
			UPDATE_PROFILE = 0x00000001,
			INTERACTIVE = 0x00000008,
			PROMPT = 0x00000010,
			REDIRECT = 0x00000080,
			LOCALDRIVE = 0x00000100,
			COMMANDLINE = 0x00000800,
			CMD_SAVECRED = 0x00001000,
		}

		// REFERENCE http://www.pinvoke.net/default.aspx/mpr.WNetUseConnection
		[StructLayout(LayoutKind.Sequential)]
		private class NETRESOURCE
		{
			public ResourceScope dwScope = 0;
			public ResourceType dwType = 0;
			public ResourceDisplayType dwDisplayType = 0;
			public ResourceUsage dwUsage = 0;

			public string lpLocalName = "";
			public string lpRemoteName = "";
			public string lpComment = "";
			public string lpProvider = "";
		}

		#endregion
	}
}
