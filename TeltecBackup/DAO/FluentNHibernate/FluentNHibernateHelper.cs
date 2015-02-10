using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Tool.hbm2ddl;
using NLog;
using System;

namespace Teltec.Backup.DAO.FluentNHibernate
{
	public static class NHibernateHelper
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static ISessionFactory _sessionFactory;
		private static Configuration _configuration;

		public static ISession OpenSession()
		{
			//
			// NOTES:
			// 1. The ISession is not threadsafe! Never access the same ISession in two concurrent threads.
			// 2. 
			//

			//Open and return the nhibernate session
			return SessionFactory.OpenSession();
		}

		public static ISessionFactory SessionFactory
		{
			get
			{
				if (_sessionFactory == null)
				{
					//Create the session factory
					_sessionFactory = Configuration.BuildSessionFactory();
				}
				return _sessionFactory;
			}
		}

		public static Configuration Configuration
		{
			get
			{
				if (_configuration == null)
				{
					//Create the nhibernate configuration
					_configuration = CreateConfiguration();
				}
				return _configuration;
			}
		}

		private static Configuration CreateConfiguration()
		{
			FluentConfiguration fluentConfig = Fluently.Configure();
			fluentConfig.Database(SQLiteConfiguration.Standard.UsingFile("database.sqlite3"));
			fluentConfig.Diagnostics(diag => diag.Enable().OutputToConsole());

			// Mappings.
			fluentConfig.Mappings(m => m.FluentMappings
				.Add<StorageAccountMap>()
				.Add<AmazonS3AccountMap>()
				.Add<BackupPlanMap>()
				.Add<BackupPlanSourceEntryMap>()
			);

			Configuration config = fluentConfig.BuildConfiguration();

			// Register interceptors.
			//config.SetInterceptor(new NHibernateAuditInterceptor());

			// Register listeners.
			ILoadEventListener[] stack = new ILoadEventListener[] {
				new NHibernateLoadListener(), // Custom listener.
				new DefaultLoadEventListener() // Keep the default listener.
			};
			config.EventListeners.LoadEventListeners = stack;

			UpdateSchema(config);
			ValidateSchema(config);

			return config;
		}

		private static bool ValidateSchema(Configuration config)
		{
			SchemaValidator validator = new SchemaValidator(config);
			try
			{
				validator.Validate();
				validator = null;
				return true;
			}
			catch (Exception ex)
			{
				logger.Fatal("Schema validation error", ex);
			}
			finally
			{
				validator = null;
			}
			return false;
		}

		private static void UpdateSchema(Configuration config)
		{
			SchemaUpdate schema = new SchemaUpdate(config);
			const bool useStdOut = true;
			const bool doUpdate = true;
			schema.Execute(useStdOut, doUpdate);
			schema = null;
		}

		public static string IdentifierPropertyName(Type type)
		{
			return SessionFactory.GetClassMetadata(type).IdentifierPropertyName;
		}

		public static string EntityName(Type type)
		{
			return NHibernateUtil.Entity(type).Name;
		}
	}
}
