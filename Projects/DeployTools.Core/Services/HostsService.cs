using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeployTools.Core.Services
{
    public class HostsService(IDbContext dbContext) : CrudService<Host>(dbContext), IHostsService
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<IList<Host>> GetHostsByInstanceId(string instanceId)
        {
            return await _dbContext.HostsRepository.GetHostsByInstanceId(instanceId);
        }

        public async Task<IList<Host>> GetHostsByAddress(string address)
        {
            return await _dbContext.HostsRepository.GetHostsByAddress(address);
        }
    }
}
