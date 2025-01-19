using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class DeploymentsService(IDbContext dbContext) : IDeploymentsService
    {
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
    }
}
