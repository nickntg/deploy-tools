using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class RdsPackagesService(IDbContext dbContext) : CrudService<RdsPackage>(dbContext), IRdsPackagesService
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<IList<RdsPackage>> GetPackagesByNameAsync(string name)
        {
            return await _dbContext.RdsPackagesRepository.GetPackagesByNameAsync(name);
        }
    }
}
