using System;
using System.Threading;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using NLog;

namespace DeployTools.Core.Services.Background
{
    public abstract class LockableJob(IDbContext dbContext, string jobType, int count)
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected JobLock Lock;

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!await TrySetLockAsync())
                {
                    return;
                }

                var jobs = await dbContext.JobsRepository.GetUnprocessedJobsOfTypeAsync(jobType, count);

                Logger.Info($"Job {jobType} - {jobs.Count} items to process");

                var successes = 0;
                var errors = 0;

                foreach (var job in jobs)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        await ProcessJobAsync(job);

                        job.IsProcessed = true;

                        await dbContext.JobsRepository.UpdateAsync(job);

                        successes++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Job {jobType} error on processing job with id {job.Id}");

                        errors++;
                    }
                }

                Logger.Info($"Job {jobType} - finished with {successes} successes and {errors} errors");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                await ReleaseLockAsync();
            }
        }

        public abstract Task ProcessJobAsync(Job job);

        private async Task<bool> TrySetLockAsync()
        {
            try
            {
                Lock = await dbContext.JobLocksRepository.SaveAsync(new JobLock
                {
                    Id = GetType().ToString()
                });

                return true;
            }
            catch
            {
                Logger.Warn($"A lock is already in place for job {GetType()} - job not run");
                return false;
            }
        }

        private async Task ReleaseLockAsync()
        {
            if (Lock is not null)
            {
                await dbContext.JobLocksRepository.DeleteAsync(Lock);
            }
        }
    }
}
