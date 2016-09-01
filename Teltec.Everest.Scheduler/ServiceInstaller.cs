using NLog;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Teltec.Everest.Scheduler
{
	[StructLayout(LayoutKind.Sequential)]
	public class ServiceStatus
	{
		public int dwServiceType = 0;
		public ServiceState dwCurrentState = 0;
		public int dwControlsAccepted = 0;
		public int dwWin32ExitCode = 0;
		public int dwServiceSpecificExitCode = 0;
		public int dwCheckPoint = 0;
		public int dwWaitHint = 0;
	}

	public class ServiceInstaller
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly int STANDARD_RIGHTS_REQUIRED = 0xF0000;
		public static readonly int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

		#region Service

		internal static class NativeMethods
		{
			#region Memory

			[DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
			public static extern void FillMemory(IntPtr destination, uint length, byte fill);

			#endregion

			[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr OpenSCManager(string machineName, string databaseName, ScmAccessRights dwDesiredAccess);

			[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceAccessRights dwDesiredAccess);

			[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceAccessRights dwDesiredAccess, int dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

			[DllImport("advapi32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool CloseServiceHandle(IntPtr hSCObject);

			[DllImport("advapi32.dll")]
			public static extern int QueryServiceStatus(IntPtr hService, ServiceStatus lpServiceStatus);

			[DllImport("advapi32.dll", SetLastError = true)]
			public static extern bool SetServiceStatus(IntPtr hService, ref ServiceStatus lpServiceStatus);

			[DllImport("advapi32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool DeleteService(IntPtr hService);

			[DllImport("advapi32.dll")]
			public static extern int ControlService(IntPtr hService, ServiceControl dwControl, ServiceStatus lpServiceStatus);

			[DllImport("advapi32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);
		}

		#endregion

		public ServiceInstaller(string serviceName)
		{
			ServiceName = serviceName;
			DisplayName = serviceName;
		}

		#region Properties

		public string[] DependsOn = null;
		public string ServiceStartName = null;
		public string Account = null;
		public string Password = null;

		#endregion

		public void Uninstall()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.AllAccess);
				if (service == IntPtr.Zero)
					throw new ApplicationException("Service not installed.");

				try
				{
					StopService(service);
					if (!NativeMethods.DeleteService(service))
						throw new ApplicationException("Could not delete service: " + GetLastErrorMessage());
				}
				finally
				{
					NativeMethods.CloseServiceHandle(service);
				}
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		public bool ServiceIsInstalled()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.QueryStatus);

				if (service == IntPtr.Zero)
					return false;

				NativeMethods.CloseServiceHandle(service);
				return true;
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		string BuildDependencies()
		{
			if (DependsOn == null)
				return null;

			StringBuilder builder = new StringBuilder();
			foreach (var dep in DependsOn)
				builder.AppendFormat("{0}\0", dep);
			return builder.ToString();
		}

		public string MachineName;
		public string BinaryPath;
		public string ServiceName;
		public string DisplayName;
		public ServiceAccessRights DesiredAccess = ServiceAccessRights.AllAccess;
		public int ServiceType = SERVICE_WIN32_OWN_PROCESS;
		public ServiceBootFlag StartType = ServiceBootFlag.AutoStart;
		public ServiceError ErrorControl = ServiceError.Normal;

		public void InstallAndStart()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.AllAccess);

				// Create it if it doesn't exist yet.
				if (service == IntPtr.Zero)
				{
					string lpDependencies = BuildDependencies();

					service = NativeMethods.CreateService(
						scm, ServiceName, DisplayName,
						DesiredAccess, ServiceType,
						StartType, ErrorControl, BinaryPath,
						null, IntPtr.Zero, lpDependencies, Account, Password);
				}

				if (service == IntPtr.Zero)
					throw new ApplicationException("Failed to install service.");

				try
				{
					StartService(service);
				}
				finally
				{
					NativeMethods.CloseServiceHandle(service);
				}
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		public void StartService()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
				if (service == IntPtr.Zero)
					throw new ApplicationException("Could not open service.");

				try
				{
					StartService(service);
				}
				finally
				{
					NativeMethods.CloseServiceHandle(service);
				}
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		public void StopService()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);
				if (service == IntPtr.Zero)
					throw new ApplicationException("Could not open service.");

				try
				{
					StopService(service);
				}
				finally
				{
					NativeMethods.CloseServiceHandle(service);
				}
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		private void StartService(IntPtr service)
		{
			ServiceStatus status = new ServiceStatus();
			bool result = NativeMethods.StartService(service, 0, 0);
			if (!result)
			{
				string message = GetLastErrorMessage();
				logger.Error(message);
				throw new ApplicationException(message);
			}

			var changedStatus = WaitForServiceStatus(service, ServiceState.StartPending, ServiceState.Running);
			if (!changedStatus)
				throw new ApplicationException("Unable to start service");
		}

		private void StopService(IntPtr service)
		{
			ServiceStatus status = new ServiceStatus();
			NativeMethods.ControlService(service, ServiceControl.Stop, status);
			var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
			if (!changedStatus)
				throw new ApplicationException("Unable to stop service");
		}

		public ServiceState GetServiceStatus()
		{
			IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

			try
			{
				IntPtr service = NativeMethods.OpenService(scm, ServiceName, ServiceAccessRights.QueryStatus);
				if (service == IntPtr.Zero)
					return ServiceState.NotFound;

				try
				{
					return GetServiceStatus(service);
				}
				finally
				{
					NativeMethods.CloseServiceHandle(service);
				}
			}
			finally
			{
				NativeMethods.CloseServiceHandle(scm);
			}
		}

		private ServiceState GetServiceStatus(IntPtr service)
		{
			ServiceStatus status = new ServiceStatus();

			if (NativeMethods.QueryServiceStatus(service, status) == 0)
				throw new ApplicationException("Failed to query service status.");

			return status.dwCurrentState;
		}

		private string GetLastErrorMessage()
		{
			string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
			return errorMessage;
		}

		//
		// "Starting a Service" by "Microsoft Corporation" is licensed under MS-PL
		//
		// Title?   Starting a Service
		// Author?  Microsoft Corporation
		// Source?  https://msdn.microsoft.com/en-us/library/windows/desktop/ms686315%28v=vs.85%29.aspx
		// License? MS-PL - https://opensource.org/licenses/MS-PL
		//
		private bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
		{
			ServiceStatus status = new ServiceStatus();

			NativeMethods.QueryServiceStatus(service, status);
			if (status.dwCurrentState == desiredStatus)
				return true;

			int dwStartTickCount = Environment.TickCount;
			long dwOldCheckPoint = status.dwCheckPoint;

			while (status.dwCurrentState == waitStatus)
			{
				// Do not wait longer than the wait hint. A good interval is
				// one tenth the wait hint, but no less than 1 second and no
				// more than 10 seconds.

				int dwWaitTime = (int)status.dwWaitHint / 10;

				if (dwWaitTime < 1000)
					dwWaitTime = 1000;
				else if (dwWaitTime > 10000)
					dwWaitTime = 10000;

				//logger.Debug("dwWaitTime = " + dwWaitTime);

				Thread.Sleep(dwWaitTime);

				// Check the status again.

				if (NativeMethods.QueryServiceStatus(service, status) == 0)
				{
					logger.Error(GetLastErrorMessage());
					break;
				}

				if (status.dwCheckPoint > dwOldCheckPoint)
				{
					// The service is making progress.
					dwStartTickCount = Environment.TickCount;
					dwOldCheckPoint = status.dwCheckPoint;
				}
				else
				{
					if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
					{
						// No progress made within the wait hint.
						break;
					}
				}
			}

			if (status.dwCurrentState != desiredStatus)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("Service not {0}\n", desiredStatus.ToString());
				sb.AppendFormat("  Current State: {0}\n", status.dwCurrentState);
				sb.AppendFormat("  Exit Code    : {0}\n", status.dwWin32ExitCode);
				sb.AppendFormat("  Check Point  : {0}\n", status.dwCheckPoint);
				sb.AppendFormat("  Wait Hint    : {0}\n", status.dwWaitHint);
				logger.Warn(sb.ToString());
			}

			return status.dwCurrentState == desiredStatus;
		}

		private IntPtr OpenSCManager(ScmAccessRights rights)
		{
			IntPtr scm = NativeMethods.OpenSCManager(MachineName, null, rights);
			if (scm == IntPtr.Zero)
				throw new ApplicationException("Could not connect to service control manager: " + GetLastErrorMessage());

			return scm;
		}
	}

	public enum ServiceState : int
	{
		Unknown = -1, // The state cannot be (has not been) retrieved.
		NotFound = 0, // The service is not known on the host server.
		Stopped = 1,
		StartPending = 2,
		StopPending = 3,
		Running = 4,
		ContinuePending = 5,
		PausePending = 6,
		Paused = 7
	}

	[Flags]
	public enum ScmAccessRights
	{
		Connect = 0x0001,
		CreateService = 0x0002,
		EnumerateService = 0x0004,
		Lock = 0x0008,
		QueryLockStatus = 0x0010,
		ModifyBootConfig = 0x0020,
		StandardRightsRequired = 0xF0000,
		AllAccess = (StandardRightsRequired | Connect | CreateService |
					 EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
	}

	[Flags]
	public enum ServiceAccessRights
	{
		QueryConfig = 0x1,
		ChangeConfig = 0x2,
		QueryStatus = 0x4,
		EnumerateDependants = 0x8,
		Start = 0x10,
		Stop = 0x20,
		PauseContinue = 0x40,
		Interrogate = 0x80,
		UserDefinedControl = 0x100,
		Delete = 0x00010000,
		StandardRightsRequired = 0xF0000,
		AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig |
					 QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
					 Interrogate | UserDefinedControl)
	}

	public enum ServiceBootFlag
	{
		Start = 0x00000000,
		SystemStart = 0x00000001,
		AutoStart = 0x00000002,
		DemandStart = 0x00000003,
		Disabled = 0x00000004
	}

	public enum ServiceControl
	{
		Stop = 0x00000001,
		Pause = 0x00000002,
		Continue = 0x00000003,
		Interrogate = 0x00000004,
		Shutdown = 0x00000005,
		ParamChange = 0x00000006,
		NetBindAdd = 0x00000007,
		NetBindRemove = 0x00000008,
		NetBindEnable = 0x00000009,
		NetBindDisable = 0x0000000A
	}

	public enum ServiceError
	{
		Ignore = 0x00000000,
		Normal = 0x00000001,
		Severe = 0x00000002,
		Critical = 0x00000003
	}
}
