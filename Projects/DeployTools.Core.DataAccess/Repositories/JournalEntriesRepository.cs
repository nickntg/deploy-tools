using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class JournalEntriesRepository(ISession session) : CrudRepository<JournalEntry>(session), IJournalEntriesRepository

    {
    }
}
