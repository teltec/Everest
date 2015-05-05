using System.Runtime.InteropServices;

namespace Teltec.Backup.Svc
{
	public static class Unmanaged
	{
		#region Trap console application events

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		public delegate bool HandlerRoutine(CtrlTypes sig);

		public enum CtrlTypes
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
