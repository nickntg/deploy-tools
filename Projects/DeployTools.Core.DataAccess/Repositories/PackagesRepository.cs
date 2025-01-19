using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class PackagesRepository(ISession session) : CrudRepository<Package>(session), IPackagesRepository
    {
        public async Task<IList<Package>> GetPackagesByNameAsync(string name)
        {
            return await Session.CreateCriteria<Package>()
                .Add(Restrictions.Eq(nameof(Package.Name), name))
                .ListAsync<Package>();
        }

        public async Task<IList<Package>> GetPackagesByDeployableLocationAsync(string deployableLocation)
        {
            return await Session.CreateCriteria<Package>()
                .Add(Restrictions.Eq(nameof(Package.DeployableLocation), deployableLocation))
                .ListAsync<Package>();
        }
    }
}
