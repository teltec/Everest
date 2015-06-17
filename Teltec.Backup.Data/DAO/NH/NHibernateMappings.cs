using FluentNHibernate.Mapping;
using NHibernate.Type;
using System;
using Teltec.Common.Extensions;
using Teltec.Storage;

namespace Teltec.Backup.Data.DAO.NH
{
	public static class IdentifyCreator
	{
		public static IdentityPart CustomGeneratedBy(this IdentityPart idMapping, string sequenceName)
		{
			switch (NHibernateHelper.DatabaseType)
			{
				default: throw new ArgumentNullException("Unhandled database type");
				case NHibernateHelper.SupportedDatabaseType.SQLEXPRESS_2012:
					return idMapping.GeneratedBy.Native(sequenceName);
				case NHibernateHelper.SupportedDatabaseType.SQLITE3:
					return idMapping.GeneratedBy.Native(sequenceName).UnsavedValue(null);
			}
		}
	}

	#region Accounts

	class StorageAccountMap : ClassMap<Models.StorageAccount>
	{
		public StorageAccountMap()
		{
			Table("storage_accounts");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_storage_accounts");

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.ReadOnly().Access.None()
				.CustomType<GenericEnumMapper<Models.EStorageAccountType>>()
				.Index("idx_type")
				;

			Map(p => p.Hostname)
				.Column("hostname")
				.Not.Nullable()
				.Length(Models.StorageAccount.HostnameMaxLen)
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
					.Length(Models.AmazonS3Account.DisplayNameMaxLen)
					;

				x.Map(p => p.AccessKey)
					.Column("access_key")
					.Not.Nullable()
					.Length(Models.AmazonS3Account.AccessKeyIdMaxLen)
					;

				x.Map(p => p.SecretKey)
					.Column("secret_key")
					.Not.Nullable()
					;

				x.Map(p => p.BucketName)
					.Column("bucket_name")
					.Not.Nullable()
					.Length(Models.AmazonS3Account.BucketNameMaxLen)
					;
			});
		}
	}

	#endregion

	#region Plan Schedule

	class PlanScheduleDayOfWeekMap : ClassMap<Models.PlanScheduleDayOfWeek>
	{
		public PlanScheduleDayOfWeekMap()
		{
			Table("plan_schedule_days_of_week");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_plan_schedule_days_of_week");

			References(fk => fk.Schedule)
				.Column("plan_schedule_id")
				.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.None()
				;

			Map(p => p.DayOfWeek)
				.Column("day_of_week")
				.CustomType<GenericEnumMapper<DayOfWeek>>()
				;
		}
	}

	class PlanScheduleMap : ClassMap<Models.PlanSchedule>
	{
		public PlanScheduleMap()
		{
			Table("plan_schedules");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_plan_schedules");

			Map(p => p.ScheduleType)
				.Column("schedule_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.ScheduleTypeEnum>>()
				;

			Map(p => p.OccursSpecificallyAt)
				.Column("occurs_specifically_at")
				;

			Map(p => p.RecurrencyFrequencyType)
				.Column("recurrency_frequency_type")
				.CustomType<GenericEnumMapper<Models.FrequencyTypeEnum>>()
				;

			Map(p => p.RecurrencyDailyFrequencyType)
				.Column("recurrency_daily_frequency_type")
				.CustomType<GenericEnumMapper<Models.DailyFrequencyTypeEnum>>()
				;

			Map(p => p.RecurrencySpecificallyAtTime)
				.Column("recurrency_specifically_at_time")
				.CustomType<TimeAsTimeSpanType>()
				;

			Map(p => p.RecurrencyTimeInterval)
				.Column("recurrency_time_interval")
				;

			Map(p => p.RecurrencyTimeUnit)
				.Column("recurrency_time_unit")
				.CustomType<GenericEnumMapper<Models.TimeUnitEnum>>()
				;

			Map(p => p.RecurrencyWindowStartsAtTime)
				.Column("recurrency_window_starts_at")
				.CustomType<TimeAsTimeSpanType>()
				;

			Map(p => p.RecurrencyWindowEndsAtTime)
				.Column("recurrency_window_ends_at")
				.CustomType<TimeAsTimeSpanType>()
				;

			HasMany(p => p.OccursAtDaysOfWeek)
				.KeyColumn("plan_schedule_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			Map(p => p.MonthlyOccurrenceType)
				.Column("monthly_occurence_type")
				.CustomType<GenericEnumMapper<Models.MonthlyOccurrenceTypeEnum>>()
				;

			Map(p => p.OccursMonthlyAtDayOfWeek)
				.Column("monthly_at_day_of_week")
				;

			Map(p => p.OccursAtDayOfMonth)
				.Column("occurs_at_day_of_month")
				;
		}
	}

	#endregion

	#region Backup

	class BackupPlanMap : ClassMap<Models.BackupPlan>
	{
		public BackupPlanMap()
		{
			string UNIQUE_KEY_NAME = "uk_name"; // (name)

			Table("backup_plans");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backup_plans");

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(Models.BackupPlan.NameMaxLen)
				.UniqueKey(UNIQUE_KEY_NAME)
				;

			Map(p => p.StorageAccountType)
				.Column("storage_account_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EStorageAccountType>>()
				.Index("idx_storage_account_type")
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
				.CustomType<GenericEnumMapper<Models.ScheduleTypeEnum>>()
				;

			References(fk => fk.Schedule)
				.Column("plan_schedule_id")
				.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.All()
				;

			Map(p => p.OriginalPlanName)
				.Column("original_plan_name")
				//.Not.Nullable()
				.Length(Models.BackupPlan.OriginalPlanNameMaxLen)
				;

			Map(p => p.LastRunAt)
				.Column("last_run_at")
				.Nullable()
				.CustomType<TimestampType>()
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

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backup_plans_source_entries");

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
 				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.Index("idx_backup_plan_id")
				.Index("idx_backup_plan_id_type")
				;

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EntryType>>()
				.Index("idx_backup_plan_id_type")
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.BackupPlanSourceEntry.PathMaxLen)
				;
		}
	}

	class BackupMap : ClassMap<Models.Backup>
	{
		public BackupMap()
		{
			Table("backups");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backups");

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.Index("idx_backup_plan_id")
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
			string UNIQUE_KEY_NAME = "uk_backuped_file_backup_file"; // (backup_id, backup_plan_file_Id)
			string INDEX_BACKUP_XFERSTATUS_FILE = "idx_backup_xferstatus_file";

			Table("backuped_files");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backuped_files");

			References(fk => fk.Backup)
				.Column("backup_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				.Index(INDEX_BACKUP_XFERSTATUS_FILE)
				;

			References(fk => fk.File)
				.Column("backup_plan_file_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				.Index(INDEX_BACKUP_XFERSTATUS_FILE)
				;

			Map(p => p.FileSize)
				.Column("file_size")
				;

			Map(p => p.FileStatus)
				.Column("file_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupFileStatus>>()
				;

			Map(p => p.FileLastWrittenAt)
				.Column("file_last_written_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;

			Map(p => p.TransferStatus)
				.Column("transfer_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<TransferStatus>>()
				.Index(INDEX_BACKUP_XFERSTATUS_FILE)
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
			string UNIQUE_KEY_PLAN_PATH = "uk_backup_plan_path";
			string UNIQUE_KEY_PLAN_PATHNODE = "uk_backup_plan_path_node";
			string INDEX_PATHNODE = "idx_path_node";

			Table("backup_plan_files");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backup_plan_files");

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_PLAN_PATH)
				.UniqueKey(UNIQUE_KEY_PLAN_PATHNODE)
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.BackupPlanSourceEntry.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_PLAN_PATH)
				;

			Map(p => p.LastSize)
				.Column("last_size")
				.Nullable()
				;

			Map(p => p.LastWrittenAt)
				.Column("last_written_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;

			Map(p => p.LastChecksum)
				.Column("last_checksum")
				.Nullable()
				.Length(20) // SHA-1 is 160 bits long (20 bytes)
				;

			Map(p => p.LastStatus)
				.Column("last_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.BackupFileStatus>>()
				;

			Map(p => p.CreatedAt)
				.Column("created_at")
				.Not.Nullable()
				.Not.Update()
				//.CustomType<TimestampType>()
				;

			Map(p => p.UpdatedAt)
				.Column("updated_at")
				.Nullable()
				.Not.Insert()
				//.CustomType<TimestampType>()
				;

			References(fk => fk.PathNode)
				.Column("path_node_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.All()
				.UniqueKey(UNIQUE_KEY_PLAN_PATHNODE)
				.Index(INDEX_PATHNODE)
				;

			HasMany(p => p.Versions)
				.KeyColumn("backup_plan_file_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;
		}
	}

	class BackupPlanPathNodeMap : ClassMap<Models.BackupPlanPathNode>
	{
		public BackupPlanPathNodeMap()
		{
			string UNIQUE_KEY_PLAN_PARENT_NAME = "uk_backup_plan_path_node"; // (backup_plan_id, parent_id, name)
			string UNIQUE_KEY_PLAN_PATH = "uk_backup_plan_path_node_path"; // (backup_plan_id, path)
			string INDEX_BACKUP_PLAN_TYPE_NAME = "idx_backup_plan_type_name"; // (backup_plan_id, type, name)

			Table("backup_plan_path_nodes");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_backup_plan_path_nodes");

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_PLAN_PARENT_NAME)
				.Index(INDEX_BACKUP_PLAN_TYPE_NAME)
				;

			References(fk => fk.Parent)
				.Column("parent_id")
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_PLAN_PARENT_NAME)
				;

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EntryType>>()
				.Index(INDEX_BACKUP_PLAN_TYPE_NAME)
				;

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(Models.BackupPlanPathNode.NameMaxLen)
				.UniqueKey(UNIQUE_KEY_PLAN_PARENT_NAME)
				.Index(INDEX_BACKUP_PLAN_TYPE_NAME)
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.BackupPlanPathNode.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_PLAN_PATH)
				;

			HasMany(p => p.SubNodes)
				.KeyColumn("parent_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasOne(fk => fk.PlanFile)
				.PropertyRef(this.GetPropertyName((Models.BackupPlanFile x) => x.PathNode))
				.Cascade.None()
				;
		}
	}

	#endregion

	#region Restore

	class RestorePlanMap : ClassMap<Models.RestorePlan>
	{
		public RestorePlanMap()
		{
			Table("restore_plans");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_restore_plans");

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(Models.RestorePlan.NameMaxLen)
				.UniqueKey("uk_name")
				;

			References(fk => fk.BackupPlan)
				.Column("backup_plan_id")
				.Not.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.None()
				;

			HasMany(p => p.SelectedSources)
				.KeyColumn("restore_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasMany(p => p.Files)
				.KeyColumn("restore_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasMany(p => p.Restores)
				.KeyColumn("backup_plan_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			Map(p => p.ScheduleType)
				.Column("schedule_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.ScheduleTypeEnum>>()
				;

			References(fk => fk.Schedule)
				.Column("plan_schedule_id")
				.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.All()
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

	class RestorePlanSourceEntryMap : ClassMap<Models.RestorePlanSourceEntry>
	{
		public RestorePlanSourceEntryMap()
		{
			// TODO: Add unique key!
			//string UNIQUE_KEY_NAME = "uk_restore_plan_source_entry";

			Table("restore_plans_source_entries");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_restore_plans_source_entries");

			References(fk => fk.RestorePlan)
				.Column("restore_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				;

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EntryType>>()
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.RestorePlanSourceEntry.PathMaxLen)
				;

			References(fk => fk.PathNode)
				.Column("path_node_id")
				.Not.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.None()
				;

			Map(p => p.Version)
				.Column("version")
				.Length(Models.RestorePlanSourceEntry.VersionMaxLen)
				;
		}
	}

	class RestoreMap : ClassMap<Models.Restore>
	{
		public RestoreMap()
		{
			Table("restores");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_restores");

			References(fk => fk.RestorePlan)
				.Column("restore_plan_id")
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
				.KeyColumn("restore_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;
		}
	}

	class RestoredFileMap : ClassMap<Models.RestoredFile>
	{
		public RestoredFileMap()
		{
			string UNIQUE_KEY_NAME = "uk_restored_files";
			string INDEX_RESTORE_XFERSTATUS_FILE = "idx_restore_xferstatus_file";

			Table("restored_files");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_restored_files");

			References(fk => fk.Restore)
				.Column("restore_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				.Index(INDEX_RESTORE_XFERSTATUS_FILE)
				;

			References(fk => fk.File)
				.Column("restore_plan_file_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_NAME)
				.Index(INDEX_RESTORE_XFERSTATUS_FILE)
				;

			Map(p => p.TransferStatus)
				.Column("transfer_status")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<TransferStatus>>()
				.Index(INDEX_RESTORE_XFERSTATUS_FILE)
				;

			Map(p => p.UpdatedAt)
				.Column("updated_at")
				.Not.Nullable()
				//.Not.Insert()
				//.CustomType<TimestampType>()
				;
		}
	}

	class RestorePlanFileMap : ClassMap<Models.RestorePlanFile>
	{
		public RestorePlanFileMap()
		{
			string UNIQUE_KEY_NAME = "uk_restore_plan_path";
			string INDEX_PATHNODE = "idx_path_node";

			Table("restore_plan_files");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_restore_plan_files");

			References(fk => fk.RestorePlan)
				.Column("restore_plan_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.UniqueKey(UNIQUE_KEY_NAME)
				.Cascade.None()
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.RestorePlanSourceEntry.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_NAME)
				;

			References(fk => fk.PathNode)
				.Column("path_node_id")
				.Not.Nullable()
				//.LazyLoad(Laziness.Proxy)
				.Cascade.None()
				.Index(INDEX_PATHNODE)
				;

			//Map(p => p.LastSize)
			//	.Column("last_size")
			//	.Nullable()
			//	;

			//Map(p => p.LastWrittenAt)
			//	.Column("last_written_at")
			//	.Nullable()
			//	//.CustomType<TimestampType>()
			//	;

			//Map(p => p.LastChecksum)
			//	.Column("last_checksum")
			//	.Nullable()
			//	.Length(20) // SHA-1 is 160 bits long (20 bytes)
			//	;

			//Map(p => p.LastStatus)
			//	.Column("last_status")
			//	.Not.Nullable()
			//	.CustomType<GenericEnumMapper<Models.RestoreFileStatus>>()
			//	;

			Map(p => p.CreatedAt)
				.Column("created_at")
				.Not.Nullable()
				.Not.Update()
				//.CustomType<TimestampType>()
				;

			//Map(p => p.UpdatedAt)
			//	.Column("updated_at")
			//	.Nullable()
			//	.Not.Insert()
			//	//.CustomType<TimestampType>()
			//	;
		}
	}

	#endregion

	#region Synchronization

	class SynchronizationMap : ClassMap<Models.Synchronization>
	{
		public SynchronizationMap()
		{
			Table("synchronizations");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_synchronizations");

			Map(p => p.StorageAccountType)
				.Column("storage_account_type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EStorageAccountType>>()
				.Index("idx_storage_account_type")
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
				.CustomType<GenericEnumMapper<SynchronizationStatus>>()
				;

			HasMany(p => p.Files)
				.KeyColumn("synchronization_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;
		}
	}

	class SynchronizationFileMap : ClassMap<Models.SynchronizationFile>
	{
		public SynchronizationFileMap()
		{
			string UNIQUE_KEY_SYNC_FILEPATH = "uk_synchronization_file_path";
			string UNIQUE_KEY_SYNC_PATHNODE = "uk_synchronization_path_node";
			string INDEX_PATHNODE = "idx_path_node";

			Table("synchronization_files");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_synchronization_files");

			References(fk => fk.Synchronization)
				.Column("synchronization_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_SYNC_FILEPATH)
				.UniqueKey(UNIQUE_KEY_SYNC_PATHNODE)
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.SynchronizationFile.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_SYNC_FILEPATH)
				;

			Map(p => p.FileSize)
				.Column("file_size")
				.Nullable()
				;

			Map(p => p.LastWrittenAt)
				.Column("last_written_at")
				.Nullable()
				//.CustomType<TimestampType>()
				;

			//Map(p => p.LastChecksum)
			//	.Column("last_checksum")
			//	.Nullable()
			//	.Length(20) // SHA-1 is 160 bits long (20 bytes)
			//	;

			//Map(p => p.LastStatus)
			//	.Column("last_status")
			//	.Not.Nullable()
			//	.CustomType<GenericEnumMapper<Models.SynchronizationFileStatus>>()
			//	;

			Map(p => p.CreatedAt)
				.Column("created_at")
				.Not.Nullable()
				.Not.Update()
				//.CustomType<TimestampType>()
				;

			Map(p => p.UpdatedAt)
				.Column("updated_at")
				.Nullable()
				.Not.Insert()
				//.CustomType<TimestampType>()
				;

			References(fk => fk.PathNode)
				.Column("path_node_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.All()
				.UniqueKey(UNIQUE_KEY_SYNC_PATHNODE)
				.Index(INDEX_PATHNODE)
				;
		}
	}

	class SynchronizationPathNodeMap : ClassMap<Models.SynchronizationPathNode>
	{
		public SynchronizationPathNodeMap()
		{
			string UNIQUE_KEY_SYNC_PARENT_NAME = "uk_synchronization_path_node"; // (synchronization_id, parent_id, name)
			string UNIQUE_KEY_SYNC_PATH = "uk_synchronization_path_node_path"; // (synchronization_id, path)
			string INDEX_SYNC_TYPE_NAME = "idx_synchronization_type_name"; // (synchronization_id, type, name)

			Table("Synchronization_path_nodes");

			Id(p => p.Id, "id").CustomGeneratedBy("seq_synchronization_path_nodes");

			References(fk => fk.Synchronization)
				.Column("synchronization_id")
				// IMPORTANT: This property cannot be `NOT NULL` because `Cascade.AllDeleteOrphan`
				// seems to set it to `NULL` before deleting the object/row.
				//.Not.Nullable()
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_SYNC_PARENT_NAME)
				.Index(INDEX_SYNC_TYPE_NAME)
				;

			References(fk => fk.Parent)
				.Column("parent_id")
				.Cascade.None()
				.UniqueKey(UNIQUE_KEY_SYNC_PARENT_NAME)
				;

			Map(p => p.Type)
				.Column("type")
				.Not.Nullable()
				.CustomType<GenericEnumMapper<Models.EntryType>>()
				.Index(INDEX_SYNC_TYPE_NAME)
				;

			Map(p => p.Name)
				.Column("name")
				.Not.Nullable()
				.Length(Models.SynchronizationPathNode.NameMaxLen)
				.UniqueKey(UNIQUE_KEY_SYNC_PARENT_NAME)
				.Index(INDEX_SYNC_TYPE_NAME)
				;

			Map(p => p.Path)
				.Column("path")
				.Not.Nullable()
				.Length(Models.SynchronizationPathNode.PathMaxLen)
				.UniqueKey(UNIQUE_KEY_SYNC_PATH)
				;

			HasMany(p => p.SubNodes)
				.KeyColumn("parent_id")
				.Cascade.AllDeleteOrphan()
				.AsBag()
				;

			HasOne(fk => fk.SyncFile)
				.PropertyRef(this.GetPropertyName((Models.SynchronizationFile x) => x.PathNode))
				.Cascade.None()
				;
		}
	}

	#endregion
}
