using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Kiukie.Tests.Integration
{
    [SetUpFixture]
    public class TestFixtureContext
    {
        [OneTimeSetUp]
        public void ApplyMigrations()
        {
            var config = LoadConfiguration();

            var connString = config.GetConnectionString("Kiukie");
            LoadServices(connString);

            using (var connection = new SqlConnection(connString))
            {
                var databaseProvider = new MssqlDatabaseProvider(connection);
                var migrator = new SimpleMigrator(typeof(TestFixtureContext).Assembly, databaseProvider);
                migrator.Load();
                migrator.MigrateToLatest();
            }
        }

        public static IServiceProvider Provider { get; private set; }

        private static IConfiguration LoadConfiguration()
        {
            var workingDirectory = Environment.CurrentDirectory;

            var builder = new ConfigurationBuilder()
                        .SetBasePath(workingDirectory)
                        .AddJsonFile("appsettings.json");

            return builder.Build();
        }
        
        private static void LoadServices(string connString)
        {
            var services = new ServiceCollection();
            services.AddTransient<IDbConnection>((provider) => new SqlConnection(connString));

            Provider = services.BuildServiceProvider();
        }
    }

    [Migration(1)]
    public class CreateQueue : Migration
    {
        protected override void Up()
        {
            Execute(@"CREATE SCHEMA Kiukie");

            Execute(@"CREATE TABLE Kiukie.Queue
                    (
                        [Id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
                        [Payload] [varchar](max) NOT NULL,
                        [CreatedDate] [datetime] NULL,
                        [StatusId] [int] NULL,
                        [UpdatedDate] [datetime] NULL,
                    )");
        }

        protected override void Down()
        {
            Execute(@"DROP TABLE Kiukie.Queue");

            Execute(@"DROP SCHEMA Kiukie");
        }
    }
}