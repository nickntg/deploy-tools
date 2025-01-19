using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class ApplicationsRepository(ISession session) : CrudRepository<Application>(session), IApplicationsRepository
    {
        private readonly ISession _session1 = session;

        public async Task<IList<Application>> GetApplicationsByNameAsync(string name)
        {
            return await _session1.CreateCriteria<Application>()
                .Add(Restrictions.Eq(nameof(Application.Name), name))
                .ListAsync<Application>();
        }

        public async Task<IList<Application>> GetApplicationsByDomainAsync(string domain)
        {
            return await _session1.CreateCriteria<Application>()
                .Add(Restrictions.Eq(nameof(Application.Domain), domain))
                .ListAsync<Application>();
        }
    }
}
