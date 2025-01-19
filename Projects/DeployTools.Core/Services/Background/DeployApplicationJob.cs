using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.Services.Background.Interfaces;
using DeployTools.Core.Services.Interfaces;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models.Background;
using System.Text.Json;

namespace DeployTools.Core.Services.Background
{
    public class DeployApplicationJob(IDbContext dbContext, 
        IHostsService hostsService,
        IApplicationsService applicationsService,
        IDeployOrchestrator deployOrchestrator) : LockableJob(dbContext, nameof(DeployApplicationJob), 10), IDeployApplicationJob
    {
        public override async Task ProcessJobAsync(Job job)
        {
            var jobInfo = JsonSerializer.Deserialize<DeployJobInfo>(job.SerializedInfo);

            var host = await hostsService.GetByIdAsync(jobInfo.HostId);

            if (host is null)
            {
                Logger.Warn(
                    $"Could not find host with id {jobInfo.HostId} to deploy application with id {jobInfo.ApplicationId}");
                return;
            }

            var application = await applicationsService.GetByIdAsync(jobInfo.ApplicationId);

            if (application is null)
            {
                Logger.Warn($"Could not find application with id {jobInfo.ApplicationId} to deploy to host with id {jobInfo.HostId}");
                return;
            }

            await deployOrchestrator.DeployAsync(application, host);
        }
    }
}
