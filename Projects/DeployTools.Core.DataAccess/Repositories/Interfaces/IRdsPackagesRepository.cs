using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IRdsPackagesRepository : ICrudRepository<RdsPackage>
    {
        Task<IList<RdsPackage>> GetPackagesByNameAsync(string name);
    }
}
