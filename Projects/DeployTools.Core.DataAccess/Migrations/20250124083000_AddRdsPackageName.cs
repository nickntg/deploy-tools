using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501240830)]
	public class _20250124083000_AddRdsPackageName : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("rds_packages")
                .AddColumn("name").AsString().NotNullable();
        }
	}
}