using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501231720)]
	public class _20250123172000_AddRdsEngine : BaseMigration
	{
		public override void Up()
        {
            CreateBaseColumnsAndIndexes("rds_engines", 32)
                .WithColumn("engine_name").AsString().NotNullable()
                .WithColumn("engine_version").AsString().NotNullable()
                .WithColumn("status").AsString().NotNullable();
        }
	}
}