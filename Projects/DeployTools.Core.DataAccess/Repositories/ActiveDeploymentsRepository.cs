using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class ActiveDeploymentsRepository(ISession session) : CrudRepository<ActiveDeployment>(session), IActiveDeploymentsRepository
    {
        private readonly ISession _session = session;

        public async Task CleanupDeploymentsOfApplicationAsync(string applicationId)
        {
            await _session.Query<ActiveDeployment>()
                .Where(x => x.ApplicationId.Equals(applicationId))
                .DeleteAsync();
            await _session.FlushAsync();
        }

        public async Task<IList<ActiveDeployment>> GetDeploymentsOfApplicationAsync(string applicationId)
        {
            return await _session.CreateCriteria<ActiveDeployment>()
                .Add(Restrictions.Eq(nameof(ActiveDeployment.ApplicationId), applicationId))
                .ListAsync<ActiveDeployment>();
        }
    }
}
