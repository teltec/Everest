using System;

namespace PostInstaller.Databases
{
	public class SQLite3 : IDatabaseEngine
	{
		public bool CreateDatabase()
		{
			// NHibernate SUPPORTS automatic creation of SQLite3 databases.
			return true;
		}

		public bool DropDatabase()
		{
			throw new NotImplementedException();
		}
	}
}
