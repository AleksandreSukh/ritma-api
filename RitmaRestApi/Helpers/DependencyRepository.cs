using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using ConfigNet;
using RitmaRestApi.DataSource;
using TextLoggerNet.Interfaces;
using TextLoggerNet.Loggers;

namespace RitmaRestApi.Helpers
{
    public class Configuration : DbMigrationsConfiguration<SourceDbContextMssql>
    {
        protected override void Seed(RitmaRestApi.DataSource.SourceDbContextMssql context)
        {
            //  This method will be called after migrating to the latest version.
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
    public class Initializer : MigrateDatabaseToLatestVersion<SourceDbContextMssql, Configuration>
    {
    }
    public sealed class DependencyRepository
    {
        public const string ConnectionStringName = "ConnectionStringMSSQL";
        public const string ServiceName = "RitmaRestApi";


        private static readonly Lazy<DependencyRepository> lazy =
            new Lazy<DependencyRepository>(() =>
            {
                var apiConfig = ConfigReader.ReadFromSettings<ApiConfig>();

                var defaultLogger = Debugger.IsAttached ? (ILogger)new ConsoleLoggerEasy() : new LoggerToFileDefaultEasy();
                Func<ISourceDbContext> contextProvider = () => new SourceDbContextPostgres(ConnectionStringName);
                Func<IWordDataRepository> reportRepositoryProvider = () => new WordDataRepository(contextProvider);
                return new DependencyRepository(defaultLogger, reportRepositoryProvider, contextProvider, apiConfig);
            });

        public static DependencyRepository Instance { get { return lazy.Value; } }

        private DependencyRepository(ILogger logger, Func<IWordDataRepository> reportRepositoryProvider, Func<ISourceDbContext> contextProvider, ApiConfig apiConfig)
        {
            Logger = logger;
            ReportRepositoryProvider = reportRepositoryProvider;
            ContextProvider = contextProvider;
            ApiConfig = apiConfig;
        }
        public Func<ISourceDbContext> ContextProvider { get; }
        public Func<IWordDataRepository> ReportRepositoryProvider { get; }
        public ILogger Logger { get; }
        public ApiConfig ApiConfig { get; }
    }
}
