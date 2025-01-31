using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501130908)]
	public class _20250113090800_AddVpcAndDomain : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("hosts")
                .AddColumn("vpc_id")
                .AsString()
                .Nullable();

            IfDatabase("PostgreSQL")
                .Alter
                .Table("applications")
                .AddColumn("domain")
                .AsString()
                .Nullable();
        }
	}
}