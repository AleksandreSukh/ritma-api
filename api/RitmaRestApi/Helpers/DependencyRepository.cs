using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using RitmaRestApi.DataSource;
using TextLoggerNet.Interfaces;
using TextLoggerNet.Loggers;

namespace RitmaRestApi.Helpers
{
    public class Configuration : DbMigrationsConfiguration<SourceDbContextPostgres>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
    public class Initializer : MigrateDatabaseToLatestVersion<SourceDbContextPostgres, Configuration>
    {
    }
    public sealed class DependencyRepository
    {
        public const string ConnectionStringName = "ConnectionStringMSSQL";
        public const string ServiceName = "RitmaRestApi";


        private static readonly Lazy<DependencyRepository> lazy =
            new Lazy<DependencyRepository>(() =>
            {

                var defaultLogger = Debugger.IsAttached ? (ILogger)new ConsoleLoggerEasy() : new LoggerToFileDefaultEasy();
                Func<ISourceDbContext> contextProvider = () => new SourceDbContextPostgres(ConnectionStringName);
                Func<IWordDataRepository> reportRepositoryProvider = () => new WordDataRepository(contextProvider);
                return new DependencyRepository(defaultLogger, reportRepositoryProvider, contextProvider);
            });

        public static DependencyRepository Instance { get { return lazy.Value; } }

        private DependencyRepository(ILogger logger, Func<IWordDataRepository> reportRepositoryProvider, Func<ISourceDbContext> contextProvider)
        {
            Logger = logger;
            ReportRepositoryProvider = reportRepositoryProvider;
            ContextProvider = contextProvider;
        }
        public Func<ISourceDbContext> ContextProvider { get; }
        public Func<IWordDataRepository> ReportRepositoryProvider { get; }
        public ILogger Logger { get; }

    }
}
