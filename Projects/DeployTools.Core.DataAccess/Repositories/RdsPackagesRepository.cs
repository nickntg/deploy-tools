using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class RdsPackagesRepository(ISession session) : CrudRepository<RdsPackage>(session), IRdsPackagesRepository
    {
        public async Task<IList<RdsPackage>> GetPackagesByNameAsync(string name)
        {
            return await Session.CreateCriteria<RdsPackage>()
                .Add(Restrictions.Eq(nameof(RdsPackage.Name), name))
                .ListAsync<RdsPackage>();
        }
    }
}
