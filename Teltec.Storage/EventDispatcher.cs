using System;
using System.Windows.Threading; // Requires WindowsBase assembly.

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
	}
}
