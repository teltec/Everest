using SimpleInjector;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Backup.Ipc.TcpSocket;
using Teltec.Common.Threading;

namespace Teltec.Backup.App
{
	public class Provider
	{
		private static Container _Container = new Container();
		public static Container Container
		{
			get { return _Container; }
			private set { _Container = value; }
		}

		private static System.ComponentModel.ISynchronizeInvoke SynchronizingObject = new MockSynchronizeInvoke();
		private static GuiHandler _Handler = new GuiHandler(SynchronizingObject, Commands.GUI_CLIENT_NAME, Commands.IPC_DEFAULT_HOST, Commands.IPC_DEFAULT_PORT);
		public static GuiHandler Handler
		{
			get { return _Handler; }
			private set { _Handler = value; }
		}

		//private static DatabaseContext _DBContext = new DatabaseContext();
		//public static DatabaseContext DBContext
		//{
		//	get { return _DBContext; }
		//	private set { _DBContext = value; }
		//}

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
