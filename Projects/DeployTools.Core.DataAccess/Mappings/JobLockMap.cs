using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class JobLockMap : BaseMap<JobLock>
    {
        public JobLockMap()
        {
            Table("job_locks");
            MapBase();
        }
    }
}
