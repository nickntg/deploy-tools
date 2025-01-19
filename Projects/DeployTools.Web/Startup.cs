using System;
using Amazon.ElasticLoadBalancingV2;
using DeployTools.Core.DataAccess.Configuration;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Extensions.Logging;

namespace DeployTools.Web
{
    public class Startup(IConfiguration configuration)
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddCors();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHibernateWebContext(connectionString);

            services.AddLogging(builder =>
            {
                builder.AddNLog();
            });

            var options = configuration.GetAWSOptions();

            services.AddSingleton(_ => options.CreateServiceClient<IAmazonElasticLoadBalancingV2>());

            services.AddScoped<ICoreSsh, CoreSsh>();
            services.AddScoped<IDeployOrchestrator, DeployOrchestrator>();
            services.AddScoped<IHostsService, HostsService>();
            services.AddScoped<IDeploymentsService, DeploymentsService>();
            services.AddScoped<IPackagesService, PackagesService>();
            services.AddScoped<IApplicationsService, ApplicationsService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            try
            {
                serviceProvider.PerformDatabaseMigrations();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not perform database migrations");
                Environment.FailFast(string.Empty, ex);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                        "Cache-Control", "max-age=604800, must-revalidate");
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
