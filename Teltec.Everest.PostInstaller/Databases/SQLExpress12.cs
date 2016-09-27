/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using Teltec.Common.Extensions;

namespace PostInstaller.Databases
{
	public class SQLExpress12 : IDatabaseEngine
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		DatabaseConfig Config;
		string FilePath;
		bool Verbose;

		public SQLExpress12(DatabaseConfig config, bool verbose = false)
		{
			Config = config;
			FilePath = Config.DatabaseCreateScriptPath;
			Verbose = verbose;
		}

		public string ExpandSQL(string sql)
		{
			// Expand variables in SQL.
			var variables = new StringDictionary();

			//variables.Add("database_dir", Config.BinaryDirectory);
			variables.Add("database_name", Config.DatabaseName);
			variables.Add("schema_name", Config.DatabaseSchemaName);
			variables.Add("user_role", Config.DatabasUserRole);
			variables.Add("username", Config.DatabaseUserName);
			variables.Add("password", Config.DatabasePassword);
			sql = sql.ExpandVariables(variables);

			return sql;
		}

		public bool CreateDatabase()
		{
			return ExecuteSQLScript(Config.DatabaseCreateScriptPath);
		}

		public bool DropDatabase()
		{
			return ExecuteSQLScript(Config.DatabaseDropScriptPath);
		}

		private bool ExecuteSQLScript(string scriptPath)
		{
			if (ConnectionString == null)
				ConnectionString = Config.PrivilegedConnectionStringFallback;

			string sql = null;
			try
			{
				sql = File.ReadAllText(scriptPath);
			}
			catch (Exception ex)
			{
				logger.Error("The file {0} could not be read: {1}", FilePath, ex.Message);
				return false;
			}

			sql = ExpandSQL(sql);
			if (Verbose)
				Console.WriteLine(sql);

			SqlConnection connection = new SqlConnection(ConnectionString);
			try
			{
				connection.Open();
				string[] statements = SplitStatements(sql);
				foreach (string statement in statements)
				{
					if (string.IsNullOrEmpty(statement.Trim()))
						continue;

					if (Verbose)
						Console.WriteLine("EXECUTING: {0}", statement);

					SqlCommand command = new SqlCommand(statement, connection);
					command.ExecuteNonQuery();
				}
			}
			catch (System.Exception ex)
			{
				logger.Error("Caught an exception: {0}", ex.Message);
				logger.Log(LogLevel.Error, ex, "Exception:\n---\n");
				return false;
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			return true;
		}

		private string[] SplitStatements(string statements)
		{
			var stmtList = Regex.Split(statements, @"(?mi)^\s*(?:GO|;)\s*$");
			return stmtList;
		}

		private void DumpConnectionStrings()
		{
			ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

			if (settings != null)
			{
				Console.WriteLine("Found these:");
				foreach (ConnectionStringSettings cs in settings)
					Console.WriteLine("  \"{0}\" => \"{1}\"", cs.Name, cs.ConnectionString);
			}
		}

		private string _ConnectionString = null;
		private string ConnectionString
		{
			get
			{
				if (_ConnectionString != null)
					return _ConnectionString;

				ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[Config.PrivilegedConnectionStringName];
				if (connectionSettings == null)
				{
					logger.Warn("Couldn't find connectionString {0}.", Config.PrivilegedConnectionStringName);
					DumpConnectionStrings();
					return null;
				}

				string connectionString = connectionSettings.ConnectionString;
				if (string.IsNullOrEmpty(connectionString))
				{
					logger.Warn("The connectionString {0} is invalid.", Config.PrivilegedConnectionStringName);
					DumpConnectionStrings();
					return null;
				}

				_ConnectionString = connectionString;
				return _ConnectionString;
			}
			set
			{
				_ConnectionString = value;
			}
		}

		/*
		private string GetDataPathFromRegistry()
		{
			string keyName1 = @"SOFTWARE\Microsoft\Microsoft SQL Server\SQLEXPRESS";
			using (RegistryKey subKey1 = Registry.LocalMachine.OpenSubKey(keyName1))
			{
				if (subKey1 == null)
					throw new ApplicationException(string.Format("Could not find registry key {0}", keyName1));
				RegistryKey subKey2 = subKey1.OpenSubKey("Setup");
				if (subKey2 == null)
					throw new ApplicationException(string.Format("Could not find registry key {0}", subKey2.ToString()));
				string result = subKey2.GetValue("SQLPath").ToString();
				return string.IsNullOrEmpty(result) ? null : result + @"\DATA\";
			}
		}
		*/
	}
}
