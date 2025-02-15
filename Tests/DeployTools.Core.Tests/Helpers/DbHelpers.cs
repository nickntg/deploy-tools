using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using FakeItEasy;

namespace DeployTools.Core.Tests.Helpers
{
    public static class DbHelpers
    {
        public static IDbContext New()
        {
            var dbContext = A.Fake<IDbContext>(x => x.Strict());
            A.CallTo(() => dbContext.ActiveDeploymentsRepository)
                .Returns(A.Fake<IActiveDeploymentsRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.ApplicationDeploysRepository)
                .Returns(A.Fake<IApplicationDeploysRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.ApplicationsRepository)
                .Returns(A.Fake<IApplicationsRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.HostsRepository)
                .Returns(A.Fake<IHostsRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.JobLocksRepository)
                .Returns(A.Fake<IJobLocksRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.JobsRepository)
                .Returns(A.Fake<IJobsRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.JournalEntriesRepository)
                .Returns(A.Fake<IJournalEntriesRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.PackagesRepository)
                .Returns(A.Fake<IPackagesRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.RdsPackagesRepository)
                .Returns(A.Fake<IRdsPackagesRepository>(x => x.Strict()));
            A.CallTo(() => dbContext.CertificateRepository)
                .Returns(A.Fake<ICertificatesRepository>(x => x.Strict()));
            return dbContext;
        }
    }
}
