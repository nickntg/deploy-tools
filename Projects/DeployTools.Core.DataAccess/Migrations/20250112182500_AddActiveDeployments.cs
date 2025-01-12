using FluentMigrator;
using DeployTools.Core.DataAccess.Migrations;

namespace AccountsApi.Core.DataAccess.Migrations
{
	[Migration(202501121825)]
	public class _20250112182500_AddActiveDeployments : BaseMigration
	{
		public override void Up()
        {
            CreateBaseColumnsAndIndexes("active_deployments")
                .WithColumn("package_id").AsString(32).NotNullable()
                .WithColumn("application_id").AsString(32).NotNullable()
                .WithColumn("host_id").AsString(32).NotNullable()
                .WithColumn("deploy_id").AsString(32).NotNullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deployments_application_id")
                .OnTable("active_deployments")
                .OnColumn("application_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deployments_host_id")
                .OnTable("active_deployments")
                .OnColumn("host_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deployments_package_id")
                .OnTable("active_deployments")
                .OnColumn("package_id");
        }
	}
}