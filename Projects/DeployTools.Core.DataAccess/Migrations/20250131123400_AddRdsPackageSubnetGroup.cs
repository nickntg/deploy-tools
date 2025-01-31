using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501311234)]
	public class _20250131123400_AddRdsPackageSubnetGroup : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("rds_packages")
                .AddColumn("db_subnet_group_name").AsString().Nullable().WithDefaultValue("default");
        }
	}
}