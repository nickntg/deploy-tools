using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IJobsRepository : ICrudRepository<Job>
    {
        Task<IList<Job>> GetUnprocessedJobsOfTypeAsync(string type, int count);
    }
}
