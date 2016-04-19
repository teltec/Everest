using System;
using System.Security.Permissions; // Requires WindowsBase assembly.
using System.Windows.Threading;

namespace Teltec.Storage
{
	// Summary:
	//     Provides services for managing the queue of work items for a thread.
	public sealed class EventDispatcher
	{
		private Dispatcher _Dispatcher;

		public EventDispatcher()
		{
			// Gets the Dispatcher for the thread currently executing
			// and creates a new Dispatcher if one is not already
			// associated with the thread.
			_Dispatcher = Dispatcher.CurrentDispatcher;
		}

		public void Invoke(Action callback)
		{
			_Dispatcher.Invoke(callback);
		}

		public TResult Invoke<TResult>(Func<TResult> callback)
		{
			return _Dispatcher.Invoke(callback);
		}

		#region Code from MSDN

		//
		// "DispatcherFrame Class" by "Microsoft Corporation" is licensed under MS-PL
		//
		// Title?   DispatcherFrame Class
		// Author?  Microsoft Corporation
		// Source?  https://msdn.microsoft.com/en-us/library/system.windows.threading.dispatcherframe.aspx
		// License?  MS-PL - https://opensource.org/licenses/MS-PL
		//

		[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public void DoEvents()
		{
			DispatcherFrame frame = new DispatcherFrame();
			_Dispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback(ExitFrame), frame);
			Dispatcher.PushFrame(frame);
		}

		private static object ExitFrame(object frame)
		{
			((DispatcherFrame)frame).Continue = false;
			return null;
		}

		#endregion
	}
}
