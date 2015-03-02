namespace Teltec.Backup.App.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBackupPlanAndRelated : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.backup_plans",
                c => new
                    {
                        id = c.Guid(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 128),
                        storage_account_type = c.Int(nullable: false),
                        storage_account_id = c.Guid(nullable: false),
                        ScheduleType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.name, unique: true, name: "ix_name")
                .Index(t => t.storage_account_type, name: "ix_sto_acc_type");
            
            CreateTable(
                "dbo.backup_plan_source_entries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        type = c.Int(nullable: false),
                        path = c.String(nullable: false, maxLength: 1024),
                        backup_plan_id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.backup_plans", t => t.backup_plan_id, cascadeDelete: true)
                .Index(t => t.backup_plan_id);
            
            AddColumn("dbo.accounts_amazon_s3", "type", c => c.Int(nullable: false));
            CreateIndex("dbo.accounts_amazon_s3", "type", name: "ix_type");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.backup_plan_source_entries", "backup_plan_id", "dbo.backup_plans");
            DropIndex("dbo.backup_plan_source_entries", new[] { "backup_plan_id" });
            DropIndex("dbo.backup_plans", "ix_sto_acc_type");
            DropIndex("dbo.backup_plans", "ix_name");
            DropIndex("dbo.accounts_amazon_s3", "ix_type");
            DropColumn("dbo.accounts_amazon_s3", "type");
            DropTable("dbo.backup_plan_source_entries");
            DropTable("dbo.backup_plans");
        }
    }
}
