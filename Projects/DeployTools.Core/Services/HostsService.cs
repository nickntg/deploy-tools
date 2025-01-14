using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class HostsService(IDbContext dbContext) : CrudService<Host>(dbContext), IHostsService
    {
    }
}
