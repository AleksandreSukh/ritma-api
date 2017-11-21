using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using RitmaRestApi.DataSource;
using TextLoggerNet.Interfaces;
using TextLoggerNet.Loggers;

namespace RitmaRestApi.Helpers
{
    public class Configuration : DbMigrationsConfiguration<ReportsDbContextPostgres>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
    public class Initializer : MigrateDatabaseToLatestVersion<ReportsDbContextPostgres, Configuration>
    {
    }
    public sealed class DependencyRepository
    {
        public const string ConnectionStringName = "ConnectionStringPostgres";
        private static readonly Lazy<DependencyRepository> lazy =
            new Lazy<DependencyRepository>(() =>
            {

                var defaultLogger = Debugger.IsAttached ? (ILogger)new ConsoleLoggerEasy() : new LoggerToFileDefaultEasy();
                Func<IReportsDbContext> contextProvider = () => new ReportsDbContextPostgres(ConnectionStringName);
                Func<IReportRepository> reportRepositoryProvider = () => new ReportRepository(contextProvider);
                return new DependencyRepository(defaultLogger, reportRepositoryProvider, contextProvider);
            });

        public static DependencyRepository Instance { get { return lazy.Value; } }

        private DependencyRepository(ILogger logger, Func<IReportRepository> reportRepositoryProvider, Func<IReportsDbContext> contextProvider)
        {
            Logger = logger;
            ReportRepositoryProvider = reportRepositoryProvider;
            ContextProvider = contextProvider;
        }
        public Func<IReportsDbContext> ContextProvider { get; }
        public Func<IReportRepository> ReportRepositoryProvider { get; }
        public ILogger Logger { get; }

    }
}
