using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IDeploymentsService
    {
        Task<IList<ActiveDeployment>> GetAllActiveDeploymentsAsync();
        Task<IList<ActiveDeployment>> GetActiveDeploymentsOfHostAsync(string hostId);
        Task<IList<ActiveDeployment>> GetActiveDeploymentsOfPackageAsync(string packageId);
        Task<IList<ActiveDeployment>> GetActiveDeploymentsOfApplicationAsync(string applicationId);
        Task StartTakeDownAsync(string activeDeploymentId);
        Task StartDeploymentAsync(string applicationId, string hostId);
        Task<ActiveDeployment> GetActiveDeploymentByIdAsync(string activeDeploymentId);
        Task<IList<JournalEntry>> GetJournalEntriesOfDeployAsync(string deployId);
    }
}
