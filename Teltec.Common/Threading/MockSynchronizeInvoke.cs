using System;
using System.ComponentModel;
using System.Threading;

namespace Teltec.Common.Threading
{
	public class AsyncResult : IAsyncResult
	{
		internal object _AsyncState = null;
		public object AsyncState
		{
			get { return _AsyncState; }
			internal set { _AsyncState = value; }
		}

		internal ManualResetEvent _AsyncWaitHandle = new ManualResetEvent(false);
		public WaitHandle AsyncWaitHandle
		{
			get { return _AsyncWaitHandle; }
		}

		public bool CompletedSynchronously
		{
			get { return false; }
		}

		internal int _IsCompleted = 0;
		public bool IsCompleted
		{
			get { return _IsCompleted == 1; }
			internal set { _IsCompleted = value ? 1 : 0; }
		}

		internal Exception _Exception;
		public Exception Exception
		{
			get { return _Exception; }
			internal set { _Exception = value; }
		}
	}

	//
	// REFERENCE: http://blogs.msdn.com/b/jaredpar/archive/2008/01/07/isynchronizeinvoke-now.aspx
	//
	public class MockSynchronizeInvoke : ISynchronizeInvoke
	{
		public IAsyncResult BeginInvoke(Delegate method, object[] args)
		{
			var r = new AsyncResult();

			WaitCallback del = delegate(object unused)
			{
#if true
				try
				{
					object temp = method.DynamicInvoke(args);
					Interlocked.Exchange(ref r._AsyncState, temp);
				}
				catch (Exception ex)
				{
					Interlocked.Exchange(ref r._Exception, ex);
				}

				Interlocked.Exchange(ref r._IsCompleted, 1);
				r._AsyncWaitHandle.Set();
#else
				r.AsyncState = method.DynamicInvoke(args);
				r.IsCompleted = true;
#endif
			};

			ThreadPool.QueueUserWorkItem(del);

			return r;
		}

		public object EndInvoke(IAsyncResult result)
		{
			var r = (AsyncResult)result;
			try
			{
				if (!result.IsCompleted)
					r.AsyncWaitHandle.WaitOne();
			}
			finally
			{
				r.AsyncWaitHandle.Close();
			}

			if (r.Exception != null)
			{
				throw new Exception("Error during BeginInvoke", r.Exception);
			}

			return r.AsyncState;
		}

		public object Invoke(Delegate method, object[] args)
		{
			return method.DynamicInvoke(args);
		}

		public bool InvokeRequired
		{
			get { return false; }
		}
	}
}
