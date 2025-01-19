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
        public async Task<IList<ActiveDeployment>> GetDeploymentsOfPackageAsync(string packageId)
        {
            return await Session.CreateCriteria<ActiveDeployment>()
                .Add(Restrictions.Eq(nameof(ActiveDeployment.PackageId), packageId))
                .ListAsync<ActiveDeployment>();
        }

        public async Task CleanupDeploymentsOfApplicationAsync(string applicationId)
        {
            await Session.Query<ActiveDeployment>()
                .Where(x => x.ApplicationId.Equals(applicationId))
                .DeleteAsync();
            await Session.FlushAsync();
        }

        public async Task<IList<ActiveDeployment>> GetDeploymentsOfHostAsync(string hostId)
        {
            return await Session.CreateCriteria<ActiveDeployment>()
                .Add(Restrictions.Eq(nameof(ActiveDeployment.HostId), hostId))
                .ListAsync<ActiveDeployment>();
        }

        public async Task<IList<ActiveDeployment>> GetDeploymentsOfApplicationAsync(string applicationId)
        {
            return await Session.CreateCriteria<ActiveDeployment>()
                .Add(Restrictions.Eq(nameof(ActiveDeployment.ApplicationId), applicationId))
                .ListAsync<ActiveDeployment>();
        }
    }
}
