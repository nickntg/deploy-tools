using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IDeployOrchestrator
    {
        Task DeployAsync(Application application, Host host);
        Task TakeDownAsync(Application application);
    }
}
