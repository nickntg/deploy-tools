using NHibernate;

namespace DeployTools.Core.DataAccess.Repositories
{
	public class Repository(ISession session)
    {
        public ISession Session { get; } = session;
    }
}