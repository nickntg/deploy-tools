using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202502140551)]
	public class _20250214055100_MarkedForDeletion : BaseMigration
	{
		public override void Up()
        {
            IfDatabase("PostgreSQL")
                .Alter
                .Table("certificates")
                .AddColumn("is_marked_for_deletion")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
        }
	}
}