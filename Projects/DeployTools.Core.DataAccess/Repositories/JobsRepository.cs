using DeployTools.Core.DataAccess.Entities;
using NHibernate;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class JobsRepository(ISession session) : CrudRepository<Job>(session), IJobsRepository
    {
        public async Task<IList<Job>> GetUnprocessedJobsOfTypeAsync(string type, int count)
        {
            return await Session.CreateCriteria<Job>()
                .Add(Restrictions.Eq(nameof(Job.IsProcessed), false))
                .Add(Restrictions.Eq(nameof(Job.Type), type))
                .AddOrder(new Order(nameof(Job.CreatedAt), true))
                .SetMaxResults(count)
                .ListAsync<Job>();
        }
    }
}
