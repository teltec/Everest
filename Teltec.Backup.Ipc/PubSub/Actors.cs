using ProtobufSockets;
using System;
using System.Net;

namespace Teltec.Backup.Ipc.PubSub
{
	public abstract class OperationProgressActor : IDisposable
	{
		#region Dispose Pattern Implementation

		bool _shouldDispose = false;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					//if (obj != null)
					//{
					//	obj.Dispose();
					//	obj = null;
					//}
				}
				this._isDisposed = true;
			}
		}

		/// <summary>
		/// Disposes of all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}

	public class OperationProgressWatcher : OperationProgressActor
	{
		Subscriber Subscriber;

		public OperationProgressWatcher(ushort port)
		{
			Subscriber = new Subscriber(new[] { new IPEndPoint(IPAddress.Loopback, port) });
		}

		public void Subscribe<T>(Action<T> handler) where T : class
		{
			Subscriber.Subscribe<T>(handler);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Subscriber != null)
				{
					Subscriber.Dispose();
					Subscriber = null;
				}
			}
			base.Dispose(disposing);
		}
	}

	public class OperationProgressReporter : OperationProgressActor
	{
		Publisher Publisher;

		public OperationProgressReporter(ushort port)
		{
			Publisher = new Publisher(new IPEndPoint(IPAddress.Any, port));
		}

		public void Publish<T>(T message) where T : class
		{
			Publisher.Publish(message);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Publisher != null)
				{
					Publisher.Dispose();
					Publisher = null;
				}
			}
			base.Dispose(disposing);
		}
	}
}
