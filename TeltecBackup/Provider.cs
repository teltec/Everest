using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Backup.Models;

namespace Teltec.Backup
{
	public class Provider
	{
		private static Container _Container = new Container();
		public static Container Container
		{
			get { return _Container; }
			private set { _Container = value; }
		}

		private static DatabaseContext _DBContext = new DatabaseContext();
		public static DatabaseContext DBContext
		{
			get { return _DBContext; }
			private set { _DBContext = value; }
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
			if (DBContext != null)
			{
				DBContext.Dispose();
				DBContext = null;
			}
		}
	}
}
