using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IDeploymentsService
    {
        Task<IList<ActiveDeployment>> GetActiveDeploymentsOfHostAsync(string hostId);
        Task<IList<ActiveDeployment>> GetActiveDeploymentsOfPackageAsync(string packageId);
    }
}
