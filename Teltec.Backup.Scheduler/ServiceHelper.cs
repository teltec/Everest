using System;
using System.Configuration.Install;
using System.Reflection;
using System.Threading;

namespace Teltec.Backup.Scheduler
{
	public static class ServiceHelper
	{
		//static ServiceProcessInstaller ProcessInstaller;
		//static System.ServiceProcess.ServiceInstaller ServiceInstaller;

		public static void SelfStart(bool run = false)
		{
			string serviceName = Assembly.GetExecutingAssembly().GetName().Name;

			ServiceInstaller installer = new ServiceInstaller(serviceName);
			installer.StartService();
		}

		public static void SelfInstall(bool run = false)
		{
			ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

			//string servicePath = Assembly.GetExecutingAssembly().Location;
			//string serviceName = Assembly.GetExecutingAssembly().GetName().Name;
			//
			//ServiceInstaller installer = new ServiceInstaller(serviceName);
			//installer.DesiredAccess = ServiceAccessRights.AllAccess;
			//installer.BinaryPath = servicePath;
			//installer.DependsOn = new[] {
			//	"MSSQL$SQLEXPRESS"
			//	//"MSSQLSERVER"
			//};
			//
			//installer.InstallAndStart();
		}

		public static void SelfUninstall()
		{
			ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });

			//string serviceName = Assembly.GetExecutingAssembly().GetName().Name;

			//ServiceInstaller installer = new ServiceInstaller(serviceName);
			//installer.Uninstall();
		}

		#region Trap application termination

		/// <summary>
		///  Event set when the process is terminated.
		/// </summary>
		public static readonly ManualResetEvent TerminationRequestedEvent = new ManualResetEvent(false);

		/// <summary>
		/// Event set when the process terminates.
		/// </summary>
		public static readonly ManualResetEvent TerminationCompletedEvent = new ManualResetEvent(false);

		static Unmanaged.HandlerRoutine Handler;

		public static bool OnConsoleEvent(Unmanaged.CtrlTypes reason)
		{
			Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

			// Signal termination
			TerminationRequestedEvent.Set();

			// Wait for cleanup
			TerminationCompletedEvent.WaitOne();

			// Shutdown right away so there are no lingering threads
			Environment.Exit(1);

			// Don't run other handlers, just exit.
			return true;
		}

		public static void CatchSpecialConsoleEvents()
		{
			// NOTE: Should NOT use `Console.CancelKeyPress` because it does NOT detect some events: window closing, shutdown, etc.

			// Handle special events like: Ctrl+C, window close, kill, shutdown, etc.
			Handler += new Unmanaged.HandlerRoutine(OnConsoleEvent);

			Unmanaged.SetConsoleCtrlHandler(Handler, true);
		}

		#endregion
	}
}
