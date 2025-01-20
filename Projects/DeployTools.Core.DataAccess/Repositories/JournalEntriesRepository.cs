using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class JournalEntriesRepository(ISession session) : CrudRepository<JournalEntry>(session), IJournalEntriesRepository
    {
        public async Task<IList<JournalEntry>> GetJournalEntriesOfDeployAsync(string deployId)
        {
            return await Session.CreateCriteria<JournalEntry>()
                .Add(Restrictions.Eq(nameof(JournalEntry.DeployId), deployId))
                .AddOrder(new Order(nameof(JournalEntry.CreatedAt), true))
                .ListAsync<JournalEntry>();
        }
    }
}
