using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IRdsEnginesRepository : ICrudRepository<AwsRdsEngine>
    {
        Task ClearAsync();
        Task<IList<AwsRdsEngine>> GetAllSortedAsync();
    }
}
