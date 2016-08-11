using NLog;
using SimpleInjector;
using System;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Backup.Ipc.TcpSocket;

namespace Teltec.Backup.App
{
	public class Provider
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private static Container _Container = new Container();
		public static Container Container
		{
			get { return _Container; }
			private set { _Container = value; }
		}

		private static System.ComponentModel.ISynchronizeInvoke _SynchronizingObject = null;
		private static GuiHandler _Handler;
		public static GuiHandler Handler
		{
			get
			{
				if (_Handler == null)
					throw new InvalidOperationException("Forgot to call Provider.BuildHandler?");
				return _Handler;
			}
		}

		//private static DatabaseContext _DBContext = new DatabaseContext();
		//public static DatabaseContext DBContext
		//{
		//	get { return _DBContext; }
		//	private set { _DBContext = value; }
		//}

		public static void BuildHandler(System.ComponentModel.ISynchronizeInvoke synchronizingObject)
		{
			_SynchronizingObject = synchronizingObject;
			if (_Handler != null)
				_Handler.Dispose();

			logger.Info("Building new GuiHandler");

			_Handler = new GuiHandler(_SynchronizingObject,
				Commands.IPC_DEFAULT_GUI_CLIENT_NAME, Commands.IPC_DEFAULT_HOST, Commands.IPC_DEFAULT_PORT);
		}

		public static void Setup()
		{
			// NOTE: instances that are declared as Single should be thread-safe in a multi-threaded environment
			//Container.RegisterSingle<DbContext, DatabaseContext>();
			//Container.Register<IUserRepository, SqlUserRepository>();

			Container.Verify();
			//DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(Container));
		}

		public static void Cleanup()
		{
			//if (DBContext != null)
			//{
			//	DBContext.Dispose();
			//	DBContext = null;
			//}
		}
	}
}
