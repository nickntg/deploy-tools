using System;
using System.Threading;
using DeployTools.Batch.Hangfire;
using DeployTools.Core.Configuration;
using DeployTools.Core.Services.Background.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace DeployTools.Batch
{
	public class Startup(IConfiguration configuration)
	{
		public void ConfigureServices(IServiceCollection services)
		{
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.ConfigureDataAccess(configuration);

            services.AddLogging(builder =>
            {
                builder.AddNLog();
            });

            services.ConfigureAws(configuration);
            services.ConfigureDeployToolsServices();
            services.ConfigureDeployToolsBackgroundServices();

			ConfigureHangfireServer(services, connectionString);
		}

		private static void ConfigureHangfireServer(IServiceCollection services, string connectionString)
		{
			services.AddHangfire(configuration => configuration
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseNLogLogProvider()
				.UsePostgreSqlStorage(options =>
				{
					options.UseNpgsqlConnection(connectionString);
				}));

			services.AddHangfireServer(options =>
			{
				options.CancellationCheckInterval = TimeSpan.FromSeconds(5);
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
		{
			serviceProvider.MigrateDatabase();

			app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [new AllAuthorizationFilter()] });

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Nothing to see here. Move along.");
				});
			});

            RecurringJob.AddOrUpdate<ITakeDownApplicationJob>("TakeDownApplicationJob",
                x => x.ProcessAsync(CancellationToken.None), Cron.Minutely);
        }
	}
}