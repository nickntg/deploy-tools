using FluentMigrator;
using DeployTools.Core.DataAccess.Migrations;

namespace AccountsApi.Core.DataAccess.Migrations
{
	[Migration(202501130720)]
	public class _20250113072000_AddAssignedLoadBalancer : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("hosts")
                .AddColumn("assigned_load_balancer_arn")
                .AsString()
                .NotNullable();
        }
	}
}