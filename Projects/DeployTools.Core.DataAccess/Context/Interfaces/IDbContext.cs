using System;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;

namespace DeployTools.Core.DataAccess.Context.Interfaces
{
    public interface IDbContext : IDisposable
    {
        public IApplicationDeploysRepository ApplicationDeploysRepository { get; }
        public IApplicationsRepository ApplicationsRepository { get; }
        public IHostsRepository HostsRepository { get; }
        public IPackagesRepository PackagesRepository { get; }
        public IJournalEntriesRepository JournalEntriesRepository { get; }
        public IActiveDeploymentsRepository ActiveDeploymentsRepository { get; }
        public IJobLocksRepository JobLocksRepository { get; }
        public IJobsRepository JobsRepository { get; }
        public IRdsEnginesRepository RdsEnginesRepository { get; }
        public IRdsPackagesRepository RdsPackagesRepository { get; }

        void BeginTransaction();
        void Commit();
        void Rollback();
        ISession GetSession();
        ISession CreateNewSession();
        void ClearSession();
    }
}
