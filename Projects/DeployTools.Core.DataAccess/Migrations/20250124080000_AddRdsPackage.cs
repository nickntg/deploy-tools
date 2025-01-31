using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501240800)]
	public class _20250124080000_AddRdsPackage : BaseMigration
	{
		public override void Up()
        {
            CreateBaseColumnsAndIndexes("rds_packages", 32)
                .WithColumn("engine").AsString().NotNullable()
                .WithColumn("engine_version").AsString().NotNullable()
                .WithColumn("db_instance").AsString().NotNullable()
                .WithColumn("storage_type").AsString().Nullable()
                .WithColumn("storage_in_gigabytes").AsString().Nullable()
                .WithColumn("vpc_id").AsString().NotNullable()
                .WithColumn("vpc_security_group_id").AsString().NotNullable();

            IfDatabase("PostgreSQL")
                .Alter
                .Table("applications")
                .AddColumn("rds_package_id").AsString().Nullable();
        }
	}
}