using Amazon.ElasticLoadBalancingV2;
using DeployTools.Core.DataAccess.Configuration;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Amazon.RDS;
using DeployTools.Core.Services.Background;
using DeployTools.Core.Services.Background.Interfaces;
using NLog;

namespace DeployTools.Core.Configuration
{
    public static class Extensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IServiceCollection ConfigureDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHibernateWebContext(connectionString);

            return services;
        }

        public static IServiceCollection ConfigureAws(this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration.GetAWSOptions();

            services.AddSingleton(_ => options.CreateServiceClient<IAmazonElasticLoadBalancingV2>());
            services.AddSingleton(_ => options.CreateServiceClient<IAmazonRDS>());

            return services;
        }

        public static IServiceCollection ConfigureDeployToolsServices(this IServiceCollection services)
        {
            services.AddScoped<ICoreSsh, CoreSsh>();
            services.AddScoped<IDeployOrchestrator, DeployOrchestrator>();
            services.AddScoped<IHostsService, HostsService>();
            services.AddScoped<IDeploymentsService, DeploymentsService>();
            services.AddScoped<IPackagesService, PackagesService>();
            services.AddScoped<IApplicationsService, ApplicationsService>();
            services.AddScoped<IRdsEnginesService, RdsEnginesService>();
            services.AddScoped<IRdsPackagesService, RdsPackagesService>();
            services.AddScoped<ICertificatesService, CertificatesService>();

            return services;
        }

        public static IServiceCollection ConfigureDeployToolsBackgroundServices(this IServiceCollection services)
        {
            services.AddScoped<ITakeDownApplicationJob, TakeDownApplicationJob>();
            services.AddScoped<IDeployApplicationJob, DeployApplicationJob>();

            return services;
        }

        public static void MigrateDatabase(this IServiceProvider provider)
        {
            try
            {
                provider.PerformDatabaseMigrations();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not perform database migrations");
                Environment.FailFast(string.Empty, ex);
            }
        }
    }
}
