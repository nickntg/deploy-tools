using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202501190530)]
	public class _20250119053000_RearrangePortAndHost : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Delete
                .Column("port")
                .FromTable("applications");

            IfDatabase("PostgreSQL")
                .Delete
                .Column("host_id")
                .FromTable("applications");

            IfDatabase("PostgreSQL")
                .Alter
                .Table("active_deployments")
                .AddColumn("port")
                .AsInt32()
                .NotNullable()
                .WithDefaultValue(0);
        }
	}
}