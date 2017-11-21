using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using RitmaRestApi.Helpers;
using RitmaRestApi.Models;
using SharedTemplate;
using SQLite.CodeFirst;

namespace RitmaRestApi.DataSource
{
    public class ReportsDbContextBase : IdentityDbContext<ApplicationUser>, IReportsDbContext
    {
        public ReportsDbContextBase(string connectionString) : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            SetupEntity(modelBuilder);
        }
        void SetupEntity(DbModelBuilder modelBuilder)
        {
            // Change the name of the table to be Users instead of AspNetUsers
            modelBuilder.Entity<IdentityUser>()
                .ToTable($"{RoleNames.Users}");
            modelBuilder.Entity<ApplicationUser>()
                .ToTable($"{RoleNames.Users}");

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });
        }

        public DbSet<RitmaResult> Reports { get; set; }
        public IQueryable<ApplicationUser> UsersQueriable => Users;
        public DbContext DbContext => this;

    }

    public class ReportsDbContextMssql : ReportsDbContextBase
    {
        public ReportsDbContextMssql(string connectionString)
            : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ReportsDbContextMssql>());
            base.OnModelCreating(modelBuilder);
        }
    }
    public class ReportsDbContextPostgres : ReportsDbContextBase
    {
        //For migrations
        public ReportsDbContextPostgres() : base(DependencyRepository.ConnectionStringName)
        {
        }

        public ReportsDbContextPostgres(string connectionString)
            : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ReportsDbContextPostgres>());
            base.OnModelCreating(modelBuilder);
        }
    }

    public class ReportsDbContextSqlite : ReportsDbContextBase
    {
        public ReportsDbContextSqlite(string connectionString)
            : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new SqliteCreateDatabaseIfNotExists<ReportsDbContextSqlite>(modelBuilder));
            base.OnModelCreating(modelBuilder);
        }
    }
}