namespace Teltec.Everest.App.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBackupPlanSourceEntryId : DbMigration
    {
		public override void Up()
		{
			RenameColumn(table: "dbo.backup_plan_source_entries", name: "Id", newName: "id");
		}

		public override void Down()
		{
			RenameColumn(table: "dbo.backup_plan_source_entries", name: "id", newName: "Id");
		}
    }
}
