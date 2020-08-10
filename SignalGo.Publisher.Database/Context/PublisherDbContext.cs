using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SignalGo.Publisher.Models;
using System.Reflection;

namespace SignalGo.Publisher.DataAccessLayer.Context
{
    public class PublisherDbContext : DbContext
    {
        public DbSet<UserSettingsInfo> UserSettingsInfos  { get; set; }
        public DbSet<CategoryInfo> CategoryInfos { get; set; }
        public DbSet<ProjectInfo> ProjectInfos { get; set; }
        public DbSet<IgnoreFileInfo> IgnoreFileInfos { get; set; }

        /// <summary>
        /// make for testing purpose
        /// </summary>
        private bool IsFromTest { get; set; } = true; // true for always test!
        private string DatabaseName = "PublisherDb";

        public PublisherDbContext(bool isTest)
        {
            IsFromTest = isTest;
        }
        public PublisherDbContext()
        {


        }

        public static readonly LoggerFactory _myLoggerFactory = new LoggerFactory(
            new[]
            {
                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider(),
            });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (IsFromTest)
            {
                optionsBuilder
                    .UseLoggerFactory(_myLoggerFactory)
                    //.EnableServiceProviderCaching()
                    .UseLazyLoadingProxies()
                    .UseSqlite($"Filename={DatabaseName}Test.db", options =>
                    {
                        options
                        .MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    });
                //base.OnConfiguring(optionsBuilder);
            }
            else
            {
                optionsBuilder
                    //.EnableServiceProviderCaching()
                    .UseLazyLoadingProxies()
                    .UseSqlite($"Filename={DatabaseName}.db",
                    options =>
                    {
                        options
                        .MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    });

            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Database Tables
            // Like key's, relation's, index's and delete behavior's
            modelBuilder.Entity<UserSettingsInfo>(entity =>
            {
                entity.HasKey(key => key.Username)
                .HasAnnotation("Sqlite:Autoincrement", false);
                entity.HasIndex(index => index.Username).IsUnique();
            });
            modelBuilder.Entity<IgnoreFileInfo>(entity =>
            {
                entity.HasKey(key => key.ID)
                .HasAnnotation("Sqlite:Autoincrement", true);

                entity.HasOne(one => one.ProjectInfo)
                .WithMany(many => many.IgnoreFiles)
                .HasForeignKey(fk => fk.ProjectId)
                // delete if related entity deleted
                .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ProjectInfo>(entity =>
            {
                entity.HasKey(e => e.ID)
                .HasAnnotation("Sqlite:Autoincrement", true);
                entity.HasIndex(index => index.Name).IsUnique();
                entity.HasIndex(index => index.ProjectKey).IsUnique();

                entity.HasMany(m => m.IgnoreFiles)
                .WithOne(p => p.ProjectInfo)
                .HasForeignKey(fk => fk.ProjectId)
                // delete if related entity deleted
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Category)
                .WithMany(p => p.Projects)
                .HasForeignKey(fk => fk.CategoryId)
                // set to null if related entity deleted
                .OnDelete(DeleteBehavior.SetNull);
            });
            base.OnModelCreating(modelBuilder);

            // configure category table.
            modelBuilder.Entity<CategoryInfo>(entity =>
            {
                entity.HasKey(e => e.ID)
                .HasAnnotation("Sqlite:Autoincrement", true);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasOne(one => one.ParentCategory)
                .WithMany(many => many.SubCategories)
                .HasForeignKey(fk => fk.ParentCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
