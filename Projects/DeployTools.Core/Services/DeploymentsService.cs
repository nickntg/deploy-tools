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
    }
}
