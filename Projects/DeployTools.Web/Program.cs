using System;
using System.IO;
using Amazon.ElasticLoadBalancingV2;
using DeployTools.Core.DataAccess.Configuration;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using NLog;
using NLog.Web;

namespace DeployTools.Web
{
    public class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseNLog();
        }

        //public static void Main(string[] args)
        //{
        //    var c = SetupConfiguration();
        //    var connectionString = c.GetConnectionString("DefaultConnection");

        //    var builder = WebApplication.CreateBuilder(args);

        //    builder.Services.AddHibernateWebContext(connectionString);

        //    builder.Services.AddLogging(x =>
        //    {
        //        x.AddNLog();
        //    });

        //    var options = c.GetAWSOptions();

        //    builder.Services.AddSingleton(_ => options.CreateServiceClient<IAmazonElasticLoadBalancingV2>());

        //    builder.Services.AddScoped<ICoreSsh, CoreSsh>();
        //    builder.Services.AddScoped<IDeployOrchestrator, DeployOrchestrator>();

        //    // Add services to the container.
        //    builder.Services.AddRazorPages();

        //    var app = builder.Build();

        //    try
        //    {
        //        var container = builder.Services.BuildServiceProvider();
        //        container.PerformDatabaseMigrations();
        //        container.Dispose();
                
        //        app.Services.PerformDatabaseMigrations();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Could not perform database migrations");
        //        Environment.FailFast(string.Empty, ex);
        //    }

        //    if (!app.Environment.IsDevelopment())
        //    {
        //        app.UseExceptionHandler("/Error");
        //    }

        //    app.UseRouting();

        //    app.UseAuthorization();

        //    app.MapStaticAssets();
        //    app.MapRazorPages()
        //       .WithStaticAssets();

        //    app.Run();
        //}

        //private static IConfiguration SetupConfiguration()
        //{
        //    return new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile($"appsettings.json")
        //        .AddEnvironmentVariables()
        //        .Build();
        //}
    }
}
