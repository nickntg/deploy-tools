using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class RdsEnginesRepository(ISession session) : CrudRepository<AwsRdsEngine>(session), IRdsEnginesRepository
    {
        public async Task ClearAsync()
        {
            await Session.Query<ActiveDeployment>()
                .DeleteAsync();
            await Session.FlushAsync();
        }

        public async Task<IList<AwsRdsEngine>> GetAllSortedAsync()
        {
            return await Session.CreateCriteria<AwsRdsEngine>()
                .AddOrder(new Order(nameof(AwsRdsEngine.EngineName), true))
                .AddOrder(new Order(nameof(AwsRdsEngine.EngineVersion), true))
                .ListAsync<AwsRdsEngine>();
        }
    }
}
