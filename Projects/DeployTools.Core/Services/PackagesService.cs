using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class PackagesService(IDbContext dbContext) : CrudService<Package>(dbContext), IPackagesService
    {
        public async Task<IList<Package>> GetPackagesByNameAsync(string name)
        {
            return await dbContext.PackagesRepository.GetPackagesByNameAsync(name);
        }

        public async Task<IList<Package>> GetPackagesByDeployableLocationAsync(string name)
        {
            return await dbContext.PackagesRepository.GetPackagesByDeployableLocationAsync(name);
        }
    }
}
