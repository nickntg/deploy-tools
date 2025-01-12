using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Configuration;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.Services;
using DeployTools.Core.Services.Interfaces;
using NLog;
using NLog.Extensions.Logging;

namespace DeployTools.TestApp
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static ServiceProvider Container { get; set; }
        public static IConfiguration Configuration { get; set; }

        static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            Configuration = SetupConfiguration();
            var services = new ServiceCollection();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddHibernateWebContext(connectionString);

            services.AddLogging(builder =>
            {
                builder.AddNLog();
            });

            services.AddScoped<ICoreSsh, CoreSsh>();
            services.AddScoped<IDeployOrchestrator, DeployOrchestrator>();

            Container = services.BuildServiceProvider();

            try
            {
                Container.PerformDatabaseMigrations();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not perform database migrations");
                Environment.FailFast(string.Empty, ex);
            }

            var dbContext = GetDbContext();

            var host = await dbContext.HostsRepository.GetByIdAsync("1c0a45c3b08d414383cbba526c97a72c");
            var application = await dbContext.ApplicationsRepository.GetByIdAsync("7ff281cdf97c4a919303ad1701a3d8bc");
            
            var orchestrator = GetOrchestrator();

            await orchestrator.DeployAsync(application, host);
            await orchestrator.TakeDownAsync(application);
        }

        private static IDeployOrchestrator GetOrchestrator()
        {
            var orchestrator = Container.GetService<IDeployOrchestrator>();
            if (orchestrator is null)
            {
                throw new Exception("Could not construct deploy orchestrator");
            }

            return orchestrator;
        }

        private static IDbContext GetDbContext()
        {
            var context = Container.GetService<IDbContext>();
            if (context is null)
            {
                throw new Exception("Could not construct db context");
            }

            return context;
        }

        private static IConfiguration SetupConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error($"Global exception handler caught unexpected error: {e.ExceptionObject}");
            Environment.Exit(1);
        }
    }
}
