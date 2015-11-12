using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Teltec.Common
{
	public class ConsoleAppHelper
	{
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
