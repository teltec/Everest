namespace Teltec.Everest.App.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestMigration : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.accounts_amazon_s3", "display_name", unique: true, name: "ix_display_name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.accounts_amazon_s3", "ix_display_name");
        }
    }
}
