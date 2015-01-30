namespace Teltec.Backup.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure.Annotations;
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
            : base("name=TeltecBackup.Properties.Settings.TeltecBackupDatabaseConnectionString")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<AmazonS3Account> AmazonS3Accounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //
            // REFERENCE: http://msdn.microsoft.com/en-us/data/jj591617.aspx
            //

            // Table
            modelBuilder.Entity<AmazonS3Account>()
                .ToTable("accounts_amazon_s3")
                ;

            // Columns
            modelBuilder.Entity<AmazonS3Account>()
                .Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired()
                ;
            modelBuilder.Entity<AmazonS3Account>()
                .Property(t => t.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(AmazonS3Account.DisplayNameMaxLen)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("ix_display_name") { IsUnique = true }));
                ;
            modelBuilder.Entity<AmazonS3Account>()
                .Property(t => t.AccessKey)
                .HasColumnName("access_key")
                .HasMaxLength(AmazonS3Account.AccessKeyNameMaxLen)
                .IsRequired()
                ;
            modelBuilder.Entity<AmazonS3Account>()
                .Property(t => t.SecretKey)
                .HasColumnName("secret_key")
                .IsVariableLength()
                .IsRequired()
                ;
            modelBuilder.Entity<AmazonS3Account>()
                .Property(t => t.BucketName)
                .HasColumnName("bucket_name")
                .HasMaxLength(AmazonS3Account.BucketNameMaxLen)
                .IsRequired()
                ;

            // Constraints
            modelBuilder.Entity<AmazonS3Account>()
                .HasKey(t => t.Id)
                ;
        }
    }
}