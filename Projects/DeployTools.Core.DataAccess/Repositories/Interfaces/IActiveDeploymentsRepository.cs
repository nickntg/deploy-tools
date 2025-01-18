using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IActiveDeploymentsRepository : ICrudRepository<ActiveDeployment>
    {
        Task<IList<ActiveDeployment>> GetDeploymentsOfApplicationAsync(string applicationId);
        Task<IList<ActiveDeployment>> GetDeploymentsOfHostAsync(string hostId);
        Task CleanupDeploymentsOfApplicationAsync(string applicationId);
    }
}
