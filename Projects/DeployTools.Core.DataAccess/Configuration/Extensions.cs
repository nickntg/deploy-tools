using System;
using DeployTools.Core.DataAccess.Context;
using DeployTools.Core.DataAccess.Context.Interfaces;
using FluentMigrator.Runner;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Dialect;

namespace DeployTools.Core.DataAccess.Configuration
{
	public static class Extensions
	{
		public static IServiceCollection AddHibernateWebContext(this IServiceCollection services, string connectionString)
        {
            services.AddHibernateWithoutInterception(connectionString, "web");

            return services;
        }

		public static void PerformDatabaseMigrations(this IServiceProvider provider)
		{
			var runner = provider.GetRequiredService<IMigrationRunner>();
			runner.MigrateUp();
		}

		private static IServiceCollection AddHibernateWithoutInterception(this IServiceCollection services, string connectionString,
			string context)
		{
			var factory =
				Fluently
					.Configure()
					.Database(
						PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString)
							.Dialect<PostgreSQL82Dialect>())
					.Mappings(x => x.FluentMappings.AddFromAssemblyOf<IDbContext>())
					.CurrentSessionContext(context)
					.BuildSessionFactory();

            services.AddSingleton(factory);
            services.AddScoped<IDbContext, DbContext>();

			AddFluentMigrator(services, connectionString);

			return services;
		}

		private static void AddFluentMigrator(IServiceCollection services, string connectionString)
		{
			services.AddFluentMigratorCore()
				.ConfigureRunner(x =>
					x.AddPostgres()
						.WithGlobalConnectionString(connectionString)
						.WithGlobalCommandTimeout(TimeSpan.FromSeconds(10 * 60))
						.ScanIn(typeof(IDbContext).Assembly)
						.For.All())
				.AddLogging(x => x.AddFluentMigratorConsole())
				.BuildServiceProvider(false);
		}
	}
}