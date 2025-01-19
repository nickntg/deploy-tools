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
            var jobInfo = new TakeDownJobInfo
            {
                ActiveDeployId = activeDeploymentId
            };

            var job = new Job
            {
                IsProcessed = false,
                Type = nameof(TakeDownApplicationJob),
                SerializedInfo = JsonSerializer.Serialize(jobInfo),
            };

            await dbContext.JobsRepository.SaveAsync(job);
        }

        public async Task<ActiveDeployment> GetActiveDeploymentByIdAsync(string activeDeploymentId)
        {
            return await dbContext.ActiveDeploymentsRepository.GetByIdAsync(activeDeploymentId);
        }
    }
}
