using System;
using System.Data;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Repositories;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;

namespace DeployTools.Core.DataAccess.Context
{
    public class DbContext : IDbContext
    {
        protected ISessionFactory SessionFactory;

        protected ISession Session;

        protected ITransaction Transaction;

        public DbContext(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
            Session = CreateNewSession();

            ApplicationsRepository = new ApplicationsRepository(Session);
            ApplicationDeploysRepository = new ApplicationDeploysRepository(Session);
            HostsRepository = new HostsRepository(Session);
            PackagesRepository = new PackagesRepository(Session);
            JournalEntriesRepository = new JournalEntriesRepository(Session);
            ActiveDeploymentsRepository = new ActiveDeploymentsRepository(Session);
            JobLocksRepository = new JobLocksRepository(Session);
            JobsRepository = new JobsRepository(Session);
        }

        public IApplicationDeploysRepository ApplicationDeploysRepository { get; }
        public IApplicationsRepository ApplicationsRepository { get; }
        public IHostsRepository HostsRepository { get; }
        public IPackagesRepository PackagesRepository { get; }
        public IJournalEntriesRepository JournalEntriesRepository { get; }
        public IActiveDeploymentsRepository ActiveDeploymentsRepository { get; }
        public IJobLocksRepository JobLocksRepository { get; }
        public IJobsRepository JobsRepository { get; }

        public void BeginTransaction()
        {
            if (Transaction is { IsActive: true })
            {
                throw new InvalidOperationException("Transaction already in progress");
            }

            Transaction = Session.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void Commit()
        {
            Transaction?.Commit();
        }

        public void Rollback()
        {
            Transaction?.Rollback();
        }

        public ISession GetSession()
        {
            return Session;
        }

        public ISession CreateNewSession()
        {
            return SessionFactory.OpenSession();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Transaction?.Dispose();
            Session?.Dispose();
        }

        public void ClearSession()
        {
            Session.Flush();
            Session.Clear();
        }
    }
}