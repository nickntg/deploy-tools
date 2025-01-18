using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IPackagesRepository : ICrudRepository<Package>
    {
        Task<IList<Package>> GetPackagesByNameAsync(string name);
        Task<IList<Package>> GetPackagesByDeployableLocation(string deployableLocation);
    }
}
