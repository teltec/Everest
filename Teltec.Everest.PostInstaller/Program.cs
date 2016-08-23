using NLog;
using PostInstaller.Databases;
using System;
using Teltec.Everest.Data.DAO.NH;

namespace PostInstaller
{
	class Program
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		DatabaseConfig DatabaseConfig = DatabaseConfig.DefaultConfig.ShallowCopy();
		bool Verbose = false;
		bool DoDrop = false;
		bool DoCreate = false;

		static void Main(string[] args)
		{
			try
			{
				UnsafeMain(args);
			}
			catch (Exception ex)
			{
				if (Environment.UserInteractive)
				{
					//string message = string.Format(
					//	"Caught a fatal exception ({0}). Check the log file for more details.",
					//	ex.Message);
					//if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
					//	MessageBox.Show(message);
				}
				logger.Log(LogLevel.Fatal, ex, "Caught a fatal exception");
			}
		}

		static void UnsafeMain(string[] args)
		{
			Program program = new Program();
			int exitCode = program.Run(args);
			if (exitCode != 0)
			{
				Console.WriteLine("Press any key to continue");
				Console.ReadKey();
				Environment.Exit(exitCode);
			}
		}

		private int Run(string[] args)
		{
			int ret = 0;

			for (int i=0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-create":
					case "-c":
						DoCreate = true;
						break;
					case "-drop":
					case "-d":
						DoDrop = true;
						break;
					case "-verbose":
					case "-v":
						Verbose = true;
						break;
				}
			}

			if (DoDrop)
			{
				while (true)
				{
					logger.Warn("This will DELETE the \"{0}\" database and the \"{1}\" user.\n"
						+"Do you want to proceed? [yes|no] ",
						DatabaseConfig.DatabaseName, DatabaseConfig.DatabaseUserName);
					string answer = Console.ReadLine();
					if (answer.ToLowerInvariant() == "y" || answer.ToLowerInvariant() == "yes")
					{
						bool ok = DatabaseEngine.DropDatabase();
						if (!ok)
							ret = 1;
						else
							logger.Info("Database was successfully dropped.");
						break;
					}
					else if (answer.ToLowerInvariant() == "n" || answer.ToLowerInvariant() == "no")
					{
						logger.Info("Drop canceled. Peace!");
						break;
					}
				}
			}

			if (DoCreate)
			{
				bool ok = DatabaseEngine.CreateDatabase();
				if (!ok)
					ret = 1;
				else
					logger.Info("Database was successfully created.");
			}

			return ret;
		}

		private IDatabaseEngine _DatabaseEngine;
		private IDatabaseEngine DatabaseEngine
		{
			get
			{
				if (_DatabaseEngine != null)
					return _DatabaseEngine;

				switch (NHibernateHelper.DatabaseType)
				{
					default: throw new ArgumentException("Unhandled database type", "DatabaseType");
					case NHibernateHelper.SupportedDatabaseType.SQLEXPRESS_2012:
						{
							_DatabaseEngine = new SQLExpress12(DatabaseConfig, Verbose);
							break;
						}
					case NHibernateHelper.SupportedDatabaseType.SQLITE3:
						{
							_DatabaseEngine = new SQLite3();
							break;
						}
				}

				return _DatabaseEngine;
			}
		}
	}
}
