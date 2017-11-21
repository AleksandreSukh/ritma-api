using System;
using System.Data.Entity;
using System.Linq;
using RitmaRestApi.Models;

namespace RitmaRestApi.DataSource
{
    public interface IReportsDbContext : IDisposable
    {
        DbSet<RitmaResult> Reports { get; }
        IQueryable<ApplicationUser> UsersQueriable { get; }
        int SaveChanges();
        DbContext DbContext { get; }

    }
}