using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IJournalEntriesRepository : ICrudRepository<JournalEntry>
    {
        Task<IList<JournalEntry>> GetJournalEntriesOfDeployAsync(string deployId);
    }
}
