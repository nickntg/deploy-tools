using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class ApplicationsRepository(ISession session) : CrudRepository<Application>(session), IApplicationsRepository
    {
    }
}
