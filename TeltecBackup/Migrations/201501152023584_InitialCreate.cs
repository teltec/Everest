namespace Teltec.Backup.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.accounts_amazon_s3",
                c => new
                    {
                        id = c.Guid(nullable: false),
                        display_name = c.String(nullable: false, maxLength: 16),
                        access_key = c.String(nullable: false, maxLength: 32),
                        secret_key = c.String(nullable: false),
                        bucket_name = c.String(nullable: false, maxLength: 63),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.accounts_amazon_s3");
        }
    }
}
