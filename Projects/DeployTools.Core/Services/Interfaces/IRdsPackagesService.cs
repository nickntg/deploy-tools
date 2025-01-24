using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IRdsPackagesService : ICrudService<RdsPackage>
    {
        Task<IList<RdsPackage>> GetPackagesByNameAsync(string name);
    };
}
