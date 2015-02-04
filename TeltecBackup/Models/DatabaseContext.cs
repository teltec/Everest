namespace Teltec.Backup.Models
{
	using System;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Data.Entity;
	using System.Data.Entity.Infrastructure.Annotations;
	using System.Data.Entity.ModelConfiguration;
	using System.Linq;

    public class DatabaseContext : DbContext
    {
        // Your context has been configured to use a 'DatabaseContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'TeltecBackup.Model.DatabaseContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DatabaseContext' 
        // connection string in the application configuration file.
        public DatabaseContext()
			: base("name=TeltecBackupExpress")
        {
			// Set |DataDirectory| value
			string AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string DataDirectory = System.IO.Path.Combine(AppDataDirectory, "TeltecBackup");
			bool exists = System.IO.Directory.Exists(DataDirectory);
			if (!exists)
				System.IO.Directory.CreateDirectory(DataDirectory);
			AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<AmazonS3Account> AmazonS3Accounts { get; set; }
		public virtual DbSet<BackupPlan> BackupPlans { get; set; }
		public virtual DbSet<BackupPlanSourceEntry> BackupPlanSourceEntries { get; set; }

		//protected override void OnModelCreating(DbModelBuilder modelBuilder)
		//{
		//	var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
		//   .Where(type => !String.IsNullOrEmpty(type.Namespace))
		//   .Where(type => type.BaseType != null && type.BaseType.IsGenericType
		//		&& type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));
		//	foreach (var type in typesToRegister)
		//	{
		//		dynamic configurationInstance = Activator.CreateInstance(type);
		//		modelBuilder.Configurations.Add(configurationInstance);
		//	}
		//	base.OnModelCreating(modelBuilder);
		//}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
			new AmazonS3Account_Mapping(modelBuilder);
			new BackupPlan_Mapping(modelBuilder);
			new BackupPlanSourceEntry_Mapping(modelBuilder);
        }

		internal interface IEntityMapping<T> { }

		internal class AmazonS3Account_Mapping : IEntityMapping<AmazonS3Account>
		{
			public AmazonS3Account_Mapping(DbModelBuilder builder)
			{
				//
				// REFERENCE: http://msdn.microsoft.com/en-us/data/jj591617.aspx
				//

				EntityTypeConfiguration<AmazonS3Account> mapping = builder.Entity<AmazonS3Account>();

				// Table
				mapping.ToTable("accounts_amazon_s3");

				// Columns
				mapping
					.Property(p => p.Id)
					.HasColumnName("id")
					.IsRequired()
					;
				mapping
					.Property(p => p.DisplayName)
					.HasColumnName("display_name")
					.HasMaxLength(AmazonS3Account.DisplayNameMaxLen)
					.IsRequired()
					.HasColumnAnnotation(IndexAnnotation.AnnotationName,
						new IndexAnnotation(new IndexAttribute("ix_display_name") { IsUnique = true })
					)
					;
				mapping
					.Property(p => p.Type)
					.HasColumnName("type")
					.IsRequired()
					.HasColumnAnnotation(IndexAnnotation.AnnotationName,
						new IndexAnnotation(new IndexAttribute("ix_type"))
					)
					;
				mapping
					.Property(p => p.AccessKey)
					.HasColumnName("access_key")
					.HasMaxLength(AmazonS3Account.AccessKeyNameMaxLen)
					.IsRequired()
					;
				mapping
					.Property(p => p.SecretKey)
					.HasColumnName("secret_key")
					.IsVariableLength()
					.IsRequired()
					;
				mapping
					.Property(p => p.BucketName)
					.HasColumnName("bucket_name")
					.HasMaxLength(AmazonS3Account.BucketNameMaxLen)
					.IsRequired()
					;

				// Constraints
				mapping.HasKey(p => p.Id);
			}
		}

		internal class BackupPlan_Mapping : IEntityMapping<BackupPlan>
		{
			public BackupPlan_Mapping(DbModelBuilder builder)
			{
				EntityTypeConfiguration<BackupPlan> mapping = builder.Entity<BackupPlan>();

				// Table
				mapping.ToTable("backup_plans");

				// Columns
				mapping
					.Property(p => p.Id)
					.HasColumnName("id")
					.IsRequired()
					.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
					;
				mapping
					.Property(p => p.Name)
					.HasColumnName("name")
					.HasMaxLength(BackupPlan.NameMaxLen)
					.IsRequired()
					.HasColumnAnnotation(IndexAnnotation.AnnotationName,
						new IndexAnnotation(new IndexAttribute("ix_name") { IsUnique = true })
					)
					;
				mapping
					.Property(p => p.StorageAccountType)
					.HasColumnName("storage_account_type")
					.IsRequired()
					.HasColumnAnnotation(IndexAnnotation.AnnotationName,
						new IndexAnnotation(new IndexAttribute("ix_sto_acc_type"))
					)
					;
				mapping
					.Property(p => p.StorageAccountId)
					.HasColumnName("storage_account_id")
					.IsRequired()
					;
				mapping
					.HasMany(p => p.SelectedSources)
					.WithRequired()
					.Map(p => p.MapKey("backup_plan_id"))
					.WillCascadeOnDelete(true)
					;
				mapping
					.Property(p => p.ScheduleType)
					.HasColumnName("schedule_type")
					.IsRequired()
					;

				// Constraints
				mapping.HasKey(p => p.Id);

				// Relationships
				//mapping
				//	.HasRequired(p => p.StorageAccount)
				//	.WithMany(sa => sa.BackupPlans)
				//	.HasForeignKey(p => p.StorageAccountId)
				//	.WillCascadeOnDelete(false)
				//	;
			}
		}

		internal class BackupPlanSourceEntry_Mapping : IEntityMapping<BackupPlanSourceEntry>
		{
			public BackupPlanSourceEntry_Mapping(DbModelBuilder builder)
			{
				EntityTypeConfiguration<BackupPlanSourceEntry> mapping = builder.Entity<BackupPlanSourceEntry>();

				// Table
				mapping.ToTable("backup_plan_source_entries");

				// Columns
				mapping
					.Property(p => p.Id)
					.IsRequired()
					.HasColumnName("id")
					.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
					;
				mapping
					.Property(p => p.Type)
					.HasColumnName("type")
					.IsRequired()
					;
				mapping
					.Property(p => p.Path)
					.HasColumnName("path")
					.HasMaxLength(BackupPlanSourceEntry.PathMaxLen)
					.IsRequired()
					;

				// Constraints
				mapping.HasKey(p => p.Id);
			}
		}
    }
}