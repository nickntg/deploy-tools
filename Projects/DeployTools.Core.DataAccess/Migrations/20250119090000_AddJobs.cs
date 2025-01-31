using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501190900)]
	public class _20250119090000_AddJobs : BaseMigration
	{
		public override void Up()
        {
            CreateBaseColumnsAndIndexes("job_locks", 1024);

            CreateBaseColumnsAndIndexes("jobs")
                .WithColumn("is_processed").AsBoolean().NotNullable()
                .WithColumn("serialized_info").AsString().NotNullable()
                .WithColumn("type").AsString().NotNullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_jobs_is_processed")
                .OnTable("jobs")
                .OnColumn("is_processed");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_jobs_serialized_info")
                .OnTable("jobs")
                .OnColumn("serialized_info");
        }
	}
}