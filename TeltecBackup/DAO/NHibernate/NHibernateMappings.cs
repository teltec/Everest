using FluentNHibernate.Mapping;
using Teltec.Backup.Models;

namespace Teltec.Backup.DAO.NHibernate
{
	class StorageAccountMap : ClassMap<StorageAccount>
	{
		public StorageAccountMap()
		{
			Table("storage_accounts");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_storage_accounts").UnsavedValue(null);

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.ReadOnly().Access.None()
				.CustomType<GenericEnumMapper<EStorageAccountType>>();

			DiscriminateSubClassesOnColumn("type");
		}
	}

	class AmazonS3AccountMap : SubclassMap<AmazonS3Account>
	{
		public AmazonS3AccountMap()
		{
			DiscriminatorValue(EStorageAccountType.AmazonS3);

			Join("amazon_s3_accounts", x =>
			{
				x.KeyColumn("id");

				x.Map(p => p.DisplayName)
					.Column("display_name")
					.Not.Nullable()
					.Unique().UniqueKey("uk_display_name")
					.Length(AmazonS3Account.AccessKeyNameMaxLen);

				x.Map(p => p.AccessKey)
					.Column("access_key")
					.Not.Nullable()
					.Length(AmazonS3Account.AccessKeyNameMaxLen);

				x.Map(p => p.SecretKey)
					.Column("secret_key")
					.Not.Nullable()
					.Length(AmazonS3Account.AccessKeyNameMaxLen);

				x.Map(p => p.BucketName)
					.Column("bucket_name")
					.Not.Nullable()
					.Length(AmazonS3Account.BucketNameMaxLen);
			});
		}
	}

	class BackupPlanMap : ClassMap<BackupPlan>
	{
		public BackupPlanMap()
		{
			Table("backup_plans");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backup_plans").UnsavedValue(null);

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(BackupPlan.NameMaxLen)
				.Unique().UniqueKey("uk_name");

			Map(p => p.StorageAccountType)
				.Column("storage_account_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<EStorageAccountType>>();

			//Map(p => p.StorageAccountId)
			//	.Column("storage_account_id")
			//	.Not.Nullable();

			References(fk => fk.StorageAccount)
				.Column("storage_account_id")
				.Not.Nullable();
				//.LazyLoad(Laziness.Proxy)

			HasMany(p => p.SelectedSources)
				.KeyColumn("backup_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag();

			Map(p => p.ScheduleType)
				.Column("schedule_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<BackupPlan.EScheduleType>>();
		}
	}

	class BackupPlanSourceEntryMap : ClassMap<BackupPlanSourceEntry>
	{
		public BackupPlanSourceEntryMap()
		{
			Table("backup_plans_source_entries");

			Id(p => p.Id, "id").GeneratedBy.Native("seq_backup_plans_source_entries").UnsavedValue(null);

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<BackupPlanSourceEntry.EntryType>>();

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(BackupPlanSourceEntry.PathMaxLen);

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id");
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
 				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable();
		}
	}

}
