using System.Text.Json;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models.Background;
using DeployTools.Core.Services.Background.Interfaces;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services.Background
{
    public class TakeDownApplicationJob(IDbContext dbContext, IDeployOrchestrator deployOrchestrator) : LockableJob(dbContext, nameof(TakeDownApplicationJob), 10), ITakeDownApplicationJob 
    {
        private readonly IDbContext _dbContext = dbContext;

        public override async Task ProcessJobAsync(Job job)
        {
            var jobInfo = JsonSerializer.Deserialize<TakeDownJobInfo>(job.SerializedInfo);

            var activeDeployment = await _dbContext.ActiveDeploymentsRepository.GetByIdAsync(jobInfo.ActiveDeployId);

            if (activeDeployment is null)
            {
                Logger.Warn($"Active deployment with id {jobInfo.ActiveDeployId} not found to take down");
                return;
            }

            var application = await _dbContext.ApplicationsRepository.GetByIdAsync(activeDeployment.ApplicationId);

            if (application is null)
            {
                Logger.Warn($"Application with id {activeDeployment.ApplicationId} not found to take down");
                return;
            }

            await deployOrchestrator.TakeDownAsync(application);
        }
    }
}
