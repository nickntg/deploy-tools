using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class HostsRepository(ISession session) : CrudRepository<Host>(session), IHostsRepository
    {
        public async Task<IList<Host>> GetHostsByInstanceId(string instanceId)
        {
            return await session.CreateCriteria<Host>()
                .Add(Restrictions.Eq(nameof(Host.InstanceId), instanceId))
                .ListAsync<Host>();
        }

        public async Task<IList<Host>> GetHostsByAddress(string address)
        {
            return await session.CreateCriteria<Host>()
                .Add(Restrictions.Eq(nameof(Host.Address), address))
                .ListAsync<Host>();
        }
    }
}
