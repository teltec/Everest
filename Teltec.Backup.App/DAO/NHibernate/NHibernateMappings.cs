using FluentNHibernate.Mapping;
using Teltec.Storage;

namespace Teltec.Backup.App.DAO.NHibernate
{
	class StorageAccountMap : ClassMap<Models.StorageAccount>
	{
		public StorageAccountMap()
		{
			Table("storage_accounts");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_storage_accounts").UnsavedValue(null);

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.ReadOnly().Access.None()
				.CustomType<GenericEnumMapper<Models.EStorageAccountType>>()
				;

			DiscriminateSubClassesOnColumn("type");
		}
	}

	class AmazonS3AccountMap : SubclassMap<Models.AmazonS3Account>
	{
		public AmazonS3AccountMap()
		{
			DiscriminatorValue(Models.EStorageAccountType.AmazonS3);

			Join("amazon_s3_accounts", x =>
			{
				x.KeyColumn("id");

				x.Map(p => p.DisplayName)
					.Column("display_name")
					.Not.Nullable()
					.UniqueKey("uk_display_name")
					.Length(Models.AmazonS3Account.AccessKeyNameMaxLen)
					;

				x.Map(p => p.AccessKey)
					.Column("access_key")
					.Not.Nullable()
					.Length(Models.AmazonS3Account.AccessKeyNameMaxLen)
					;

				x.Map(p => p.SecretKey)
					.Column("secret_key")
					.Not.Nullable()
					.Length(Models.AmazonS3Account.AccessKeyNameMaxLen)
					;

				x.Map(p => p.BucketName)
					.Column("bucket_name")
					.Not.Nullable()
					.Length(Models.AmazonS3Account.BucketNameMaxLen)
					;
			});
		}
	}

	class BackupPlanMap : ClassMap<Models.BackupPlan>
	{
		public BackupPlanMap()
		{
			Table("backup_plans");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backup_plans").UnsavedValue(null);

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(Models.BackupPlan.NameMaxLen)
				.UniqueKey("uk_name")
				;

			Map(p => p.StorageAccountType)
				.Column("storage_account_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EStorageAccountType>>()
				;

			//Map(p => p.StorageAccountId)
			//	.Column("storage_account_id")
			//	.Not.Nullable()
			//	;

			References(fk => fk.StorageAccount)
				.Column("storage_account_id")
				.Not.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.None()
				;

			HasMany(p => p.SelectedSources)
				.KeyColumn("backup_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasMany(p => p.Files)
				.KeyColumn("backup_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasMany(p => p.Backups)
				.KeyColumn("backup_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			Map(p => p.ScheduleType)
				.Column("schedule_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupPlan.EScheduleType>>()
				;

			Map(p => p.LastRunAt)
				.Column("last_run_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;

			Map(p => p.LastSuccessfulRunAt)
				.Column("last_successful_run_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;
		}
	}

	class BackupPlanSourceEntryMap : ClassMap<Models.BackupPlanSourceEntry>
	{
		public BackupPlanSourceEntryMap()
		{
			Table("backup_plans_source_entries");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backup_plans_source_entries").UnsavedValue(null);

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupPlanSourceEntry.EntryType>>()
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.BackupPlanSourceEntry.PathMaxLen)
				;

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
 				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				;
		}
	}

	class BackupMap : ClassMap<Models.Backup>
	{
		public BackupMap()
		{
			Table("backups");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backus").UnsavedValue(null);

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				;

			Map(p => p.StartedAt)
				.Column("started_at")
				.Not.Nullable()
				//.CustomType<TimestampType>()
				;

			Map(p => p.FinishedAt)
				.Column("finished_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;

			Map(p => p.Status)
				.Column("status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<TransferStatus>>()
				;

			HasMany(p => p.Files)
				.KeyColumn("backup_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;
		}
	}

	class BackupedFileMap : ClassMap<Models.BackupedFile>
	{
		public BackupedFileMap()
		{
			string UNIQUE_KEY_NAME = "uk_backuped_file_backup_file";

			Table("backuped_files");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backuped_files").UnsavedValue(null);

			References(fk => fk.Backup)
				.Column("backup_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				;

			References(fk => fk.File)
				.Column("backup_plan_file_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				;

			Map(p => p.FileStatus)
				.Column("file_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupFileStatus>>()
				;

			Map(p => p.TransferStatus)
				.Column("transfer_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<TransferStatus>>()
				;

			Map(p => p.UpdatedAt)
				.Column("updated_at")
				.Not.Nullable()
				//.Not.Insert()
				//.CustomType<TimestampType>()
				;
		}
	}

	class BackupPlanFileMap : ClassMap<Models.BackupPlanFile>
	{
		public BackupPlanFileMap()
		{
			string UNIQUE_KEY_NAME = "uk_backup_plan_path";

			Table("backup_plan_files");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backup_plan_files").UnsavedValue(null);

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable();
				.UniqueKey(UNIQUE_KEY_NAME)
				.Cascade.None();

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.BackupPlanSourceEntry.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_NAME);

			Map(p => p.LastSize)
				.Column("last_size")
				.Nullable();

			Map(p => p.LastWrittenAt)
				.Column("last_written_at")
				.Nullable();
				//.CustomType<TimestampType>();

			Map(p => p.LastStatus)
				.Column("last_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupFileStatus>>();

			Map(p => p.CreatedAt)
				.Column("created_at")
				.Not.Nullable()
				.Not.Update();
				//.CustomType<TimestampType>();

			Map(p => p.UpdatedAt)
				.Column("updated_at")
				.Nullable()
				.Not.Insert();
				//.CustomType<TimestampType>();
		}
	}
}
