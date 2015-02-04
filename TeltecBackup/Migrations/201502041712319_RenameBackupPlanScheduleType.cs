namespace Teltec.Backup.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBackupPlanScheduleType : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.backup_plans", name: "ScheduleType", newName: "schedule_type");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.backup_plans", name: "schedule_type", newName: "ScheduleType");
        }
    }
}
