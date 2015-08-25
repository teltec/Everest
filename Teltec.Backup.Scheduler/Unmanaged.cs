using System.Runtime.InteropServices;

namespace Teltec.Backup.Scheduler
{
	public static class Unmanaged
	{
		#region Trap console application events

		/// <summary>
		/// Adds or removes an application-defined HandlerRoutine function from the list of handler functions for the calling process
		/// </summary>
		/// <param name="handler">A pointer to the application-defined HandlerRoutine function to be added or removed. This parameter can be NULL.</param>
		/// <param name="add">If this parameter is TRUE, the handler is added; if it is FALSE, the handler is removed.</param>
		/// <returns>If the function succeeds, the return value is true.</returns>
		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		/// <summary>
		/// The console close handler delegate.
		/// </summary>
		/// <param name="closeReason">
		/// The close reason.
		/// </param>
		/// <returns>
		/// True if cleanup is complete, false to run other registered close handlers.
		/// </returns>
		public delegate bool HandlerRoutine(CtrlTypes sig);

		public enum CtrlTypes : int
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		#endregion
	}
}
