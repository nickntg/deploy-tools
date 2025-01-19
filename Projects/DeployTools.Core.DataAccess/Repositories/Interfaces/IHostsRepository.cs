using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IHostsRepository : ICrudRepository<Host>
    {
        Task<IList<Host>> GetHostsByInstanceIdAsync(string instanceId);
        Task<IList<Host>> GetHostsByAddressAsync(string address);
    }
}
