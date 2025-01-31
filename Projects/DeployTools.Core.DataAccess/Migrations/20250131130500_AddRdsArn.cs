using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501311305)]
	public class _20250131130500_AddRdsArn : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("active_deployments")
                .AddColumn("rds_arn").AsString().Nullable();
        }
	}
}