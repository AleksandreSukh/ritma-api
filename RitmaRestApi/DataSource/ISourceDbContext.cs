using System;
using System.Data.Entity;
using System.Linq;
using RestApiBase;
using RitmaRestApi.Models;
using RitmaRestApi.Models.DataSourceModels;

namespace RitmaRestApi.DataSource
{
    public interface ISourceDbContext : IDisposable
    {
        DbSet<Word> Words { get; }
        DbSet<WordSimilarity> WordSimilarities { get; }
        IQueryable<ApplicationUser> UsersQueriable { get; }
        int SaveChanges();
        DbContext DbContext { get; }
    }
}