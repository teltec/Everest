
namespace PostInstaller
{
	public class DatabaseConfig
	{
		public static readonly DatabaseConfig DefaultConfig = new DatabaseConfig
		{
			//ProgramDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			DatabaseCreateScriptPath = "SQLExpress12.CreateDatabase.in.sql",
			DatabaseDropScriptPath = "SQLExpress12.DropDatabase.in.sql",
			PrivilegedConnectionStringFallback = @"Server=.\SQLEXPRESS;Integrated security=SSPI;Connect Timeout=10;Trusted_Connection=Yes;Database=master;",
			PrivilegedConnectionStringName = "PrivilegedConnection",
			DatabaseName = "teltec_backup_db",
			DatabaseSchemaName = "dbo",
			DatabasUserRole = "teltec_backup_role",
			DatabaseUserName = "teltec_backup_user",
			DatabasePassword = "p@55w0rd",
		};

		//public string ProgramDirectory;
		public string DatabaseCreateScriptPath;
		public string DatabaseDropScriptPath;
		public string PrivilegedConnectionStringFallback;
		public string PrivilegedConnectionStringName;
		public string DatabaseName;
		public string DatabaseSchemaName;
		public string DatabasUserRole;
		public string DatabaseUserName;
		public string DatabasePassword;

		public DatabaseConfig ShallowCopy()
		{
			return (DatabaseConfig)this.MemberwiseClone();
		}
	}
}
