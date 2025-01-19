using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Mappings.Custom;
using FluentNHibernate.Mapping;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class JobLockMap : ClassMap<JobLock>
    {
        public JobLockMap()
        {
            Table("job_locks");
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.CreatedAt).CustomType<PostgresqlTimestamptz>().Column("created_at");
            Map(x => x.UpdatedAt).CustomType<PostgresqlTimestamptz>().Column("updated_at");
        }
    }
}
