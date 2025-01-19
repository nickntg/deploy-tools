using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models.Background;
using DeployTools.Core.Services.Background;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class DeploymentsService(IDbContext dbContext) : IDeploymentsService
    {
        public async Task<IList<ActiveDeployment>> GetAllActiveDeploymentsAsync()
        {
            return await dbContext.ActiveDeploymentsRepository.GetAllAsync();
        }

        public async Task<IList<ActiveDeployment>> GetActiveDeploymentsOfHostAsync(string hostId)
        {
            return await dbContext.ActiveDeploymentsRepository.GetDeploymentsOfHostAsync(hostId);
        }

        public async Task<IList<ActiveDeployment>> GetActiveDeploymentsOfPackageAsync(string packageId)
        {
            return await dbContext.ActiveDeploymentsRepository.GetDeploymentsOfPackageAsync(packageId);
        }

        public async Task<IList<ActiveDeployment>> GetActiveDeploymentsOfApplicationAsync(string applicationId)
        {
            return await dbContext.ActiveDeploymentsRepository.GetDeploymentsOfApplicationAsync(applicationId);
        }

        public async Task StartTakeDownAsync(string activeDeploymentId)
        {
            await SaveJob(new TakeDownJobInfo
            {
                ActiveDeployId = activeDeploymentId
            }, nameof(TakeDownApplicationJob));
        }

        public async Task StartDeploymentAsync(string applicationId, string hostId)
        {
            await SaveJob(new DeployJobInfo
            {
                ApplicationId = applicationId,
                HostId = hostId
            }, nameof(DeployApplicationJob));
        }

        public async Task<ActiveDeployment> GetActiveDeploymentByIdAsync(string activeDeploymentId)
        {
            return await dbContext.ActiveDeploymentsRepository.GetByIdAsync(activeDeploymentId);
        }

        private async Task SaveJob(object jobInfo, string jobType)
        {
            var job = new Job
            {
                IsProcessed = false,
                Type = jobType,
                SerializedInfo = JsonSerializer.Serialize(jobInfo),
            };

            await dbContext.JobsRepository.SaveAsync(job);
        }
    }
}
