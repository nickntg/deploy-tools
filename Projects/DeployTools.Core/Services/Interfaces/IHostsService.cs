using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IHostsService : ICrudService<Host>
    {
        Task<IList<Host>> GetHostsByInstanceId(string instanceId);
        Task<IList<Host>> GetHostsByAddress(string address);
    }
}
