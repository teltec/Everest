using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Teltec.Backup.Svc
{
	public partial class Service : ServiceBase
	{
		#region Trap application termination

		static ManualResetEvent KeepRunning = new ManualResetEvent(false);

		static bool ConsoleCtrlHandler(Unmanaged.CtrlTypes sig)
		{
			Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

			// Allow main to finish.
			KeepRunning.Reset();

			// Shutdown right away so there are no lingering threads
			Environment.Exit(-1);

			return true;
		}

		static void HandleSpecialSystemEvents()
		{
			// NOTE: Should NOT use `Console.CancelKeyPress` because it does NOT detect some events: window closing, shutdown, etc.
			
			// Handle special events like: Ctrl+C, window close, kill, shutdown, etc.
			Unmanaged.HandlerRoutine handler = new Unmanaged.HandlerRoutine(ConsoleCtrlHandler);
			
			// Keep the handler routine alive during the execution of the program,
			// because the garbage collector will destroy it after any CTRL event.
			GC.KeepAlive(handler);
			
			Unmanaged.SetConsoleCtrlHandler(handler, true);
		}

		#endregion
		
		#region CLI options

		static bool OptionRunAsService = false;

		#endregion

		static void Main(string[] args)
		{
			//
			// Parse arguments
			//
			if (args.Contains("-svc"))
			{
				OptionRunAsService = true;
			}

			if (OptionRunAsService)
			{
				System.ServiceProcess.ServiceBase[] servicesToRun;
				servicesToRun = new ServiceBase[] { new Service() };

				System.ServiceProcess.ServiceBase.Run(servicesToRun);
			}
			else
			{
				HandleSpecialSystemEvents();

				Service instance = new Service();
				instance.OnStart(args);
				KeepRunning.WaitOne(); // Wait for a signal before proceeding.
				instance.OnStop();
			}
		}

		#region Service implementation

		static System.Timers.Timer timer;

		public Service()
		{
		}

		private bool IsDateAfter(DateTime lhs, DateTime rhs)
		{
			return lhs > rhs;
		}
		
		static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("hello");
		}

		private static void start_timer()
		{
			timer.Start();
		}

		protected override void OnStart(string[] args)
		{
			timer = new System.Timers.Timer();
			timer.Interval = 2500; //1000 * 60 * 60 * 24; // Set interval of one day 
			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
			start_timer();
		}

		protected override void OnStop()
		{
			timer.Stop();
		}

		#endregion
	}
}
