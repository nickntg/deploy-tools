using DeployTools.Core.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IPackagesService : ICrudService<Package>
    {
        Task<IList<Package>> GetPackagesByNameAsync(string name);
        Task<IList<Package>> GetPackagesByDeployableLocationAsync(string name);
    }
}
