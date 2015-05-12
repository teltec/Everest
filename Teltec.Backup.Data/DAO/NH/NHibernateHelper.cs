using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Hql.Util;
using NHibernate.Tool.hbm2ddl;
using NLog;
using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Teltec.Backup.Data.DAO.NH
{
	public static class NHibernateHelper
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		// TODO(jweyrich): When to dispose it?
		private static ISessionFactory _sessionFactory;

		// TODO(jweyrich): When to dispose it?
		private static Configuration _configuration;

		// TODO(jweyrich): When to dispose it?
		private static readonly ThreadLocal<ISession> _sessions = new ThreadLocal<ISession>();
		private static readonly ThreadLocal<IStatelessSession> _statelessSessions = new ThreadLocal<IStatelessSession>();

		public static ISession GetSession()
		{
			//
			// NOTES:
			// 1. The ISession is not threadsafe! Never access the same ISession in two concurrent threads.
			// 2. 
			//

			if (!_sessions.IsValueCreated)
			{
				logger.Debug("### Opening a new ISession");
				// Open a new NHibernate session
				_sessions.Value = SessionFactory.OpenSession();
				//_sessions.Value.FlushMode = FlushMode.Never;
			}

			//if (!_sessions.Value.IsConnected)
			//	_sessions.Value.Reconnect();

			return _sessions.Value;
		}

		public static IStatelessSession GetStatelessSession()
		{
			//
			// NOTES:
			// 1. The IStatelessSession is not threadsafe! Never access the same IStatelessSession in two concurrent threads.
			// 2. 
			//

			if (!_statelessSessions.IsValueCreated)
			{
				logger.Debug("### Opening a new stateless ISession");
				// Open a new NHibernate stateless session
				_statelessSessions.Value = SessionFactory.OpenStatelessSession();
				//_statelessSessions.Value.FlushMode = FlushMode.Never;
			}

			return _statelessSessions.Value;
		}

		public static bool IsTransient(ISession session, object obj)
		{
			ISessionFactoryImplementor sessionFactoryImpl = session.SessionFactory as ISessionFactoryImplementor;
			// Here `obj` may be an instance of an NHibernate proxy, so we cannot simply use
			// `obj.GetType().FullName`, we need to get the real underlying type.
			var persister = new SessionFactoryHelper(sessionFactoryImpl)
				.RequireClassPersister(NHibernateUtil.GetClass(obj).FullName);
			bool? yes = persister.IsTransient(obj, (ISessionImplementor)session);
			return yes ?? default(bool);
		}

		public static T Unproxy<T>(object instance) where T : class
		{
			return GetSession().GetSessionImplementation().PersistenceContext.Unproxy(instance) as T;
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

		public enum SupportedDatabaseType
		{
			SQLITE3 = 1,
			SQLEXPRESS_2012 = 2,
		}

		public static readonly SupportedDatabaseType DatabaseType = SupportedDatabaseType.SQLEXPRESS_2012;

		private static Configuration CreateConfiguration()
		{
			FluentConfiguration fluentConfig = Fluently.Configure();

			switch (DatabaseType)
			{
				case SupportedDatabaseType.SQLEXPRESS_2012:
					{
						AppDomain.CurrentDomain.SetData("DataDirectory", Application.CommonAppDataPath);

						// IMPORTANT: The database MUST ALREADY EXIST.
						fluentConfig.Database(MsSqlConfiguration.MsSql2012.ConnectionString(x =>
							x.Server(@".\SQLEXPRESS").Database("teltec_backup").TrustedConnection()).UseReflectionOptimizer());
						
						break;
					}
				case SupportedDatabaseType.SQLITE3:
					{
						string dbFilePath = Application.CommonAppDataPath + @"\database.sqlite3";
						fluentConfig.Database(SQLiteConfiguration.Standard.UsingFile(dbFilePath));
						break;
					}
			}

			//fluentConfig.Diagnostics(diag => diag.Enable().OutputToConsole());

			// Add all mappings from this assembly.
			fluentConfig.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()));

			Configuration config = fluentConfig.BuildConfiguration();
			//config.SetProperty(NHibernate.Cfg.Environment.Hbm2ddlAuto, "true");

			if (DatabaseType == SupportedDatabaseType.SQLEXPRESS_2012)
			{
				config.DataBaseIntegration(db =>
				{
					db.BatchSize = 20;
					db.Dialect<NHibernate.Dialect.MsSql2012Dialect>();
					db.Driver<NHibernate.Driver.Sql2008ClientDriver>();
					//db.HqlToSqlSubstitutions = "true 1, false 0, yes 'Y', no 'N'";
					db.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
					db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
					db.LogFormattedSql = true;
					db.LogSqlInConsole = true;
					//db.OrderInserts = true;
					//db.PrepareCommands = true;
					db.SchemaAction = SchemaAutoAction.Update;
				});
			}

			UpdateSchema(config);
			ValidateSchema(config);

			// Register interceptors.
			config.SetInterceptor(new NHibernateAuditInterceptor());

			// Register listeners.
			config.AppendListeners(ListenerType.Load, new ILoadEventListener[] {
				new NHibernateLoadListener(),
			});
			config.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] {
				new NHibernatePersistentEntityListener(),
			});
			config.AppendListeners(ListenerType.Save, new ISaveOrUpdateEventListener[] {
				new NHibernatePersistentEntityListener(),
			});
			config.AppendListeners(ListenerType.Update, new ISaveOrUpdateEventListener[] {
				new NHibernatePersistentEntityListener(),
			});

			return config;
		}

		// Summary:
		//   Check the existing tables against the mappings and throw an exception
		//   if there are differences.
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
				logger.FatalException("SCHEMA VALIDATION ERROR", ex);
				throw ex;
			}
			finally
			{
				validator = null;
			}
			//return false;
		}

		// Summary:
		//   Check every table against the mappings, and, if there are changes,
		//	 use DDL commands to update the tables so that they match the schema.
		private static void UpdateSchema(Configuration config)
		{
			SchemaUpdate schema = new SchemaUpdate(config);
			const bool useStdOut = true;
			const bool doUpdate = true;
			schema.Execute(useStdOut, doUpdate);
			if (schema.Exceptions != null)
			{
				foreach (var ex in schema.Exceptions)
				{
					logger.FatalException("SCHEMA UPDATE ERROR", ex);
					throw ex;
				}
			}
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
