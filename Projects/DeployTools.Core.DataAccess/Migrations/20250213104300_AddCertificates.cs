using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
	[Migration(202502131043)]
	public class _20250213104300_AddCertificates : BaseMigration
	{
		public override void Up()
        {
            CreateBaseColumnsAndIndexes("certificates", 32)
                .WithColumn("arn").AsString().Nullable()
                .WithColumn("certificate_id").AsString().Nullable()
                .WithColumn("is_validated").AsBoolean().NotNullable()
                .WithColumn("is_created").AsBoolean().NotNullable()
                .WithColumn("domain").AsString().NotNullable()
                .WithColumn("validation_info").AsString().Nullable()
                .WithColumn("expires_at").AsCustom("timestamp with time zone").Nullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_certificates_domain")
                .OnTable("certificates")
                .OnColumn("domain")
                .Unique();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_certificates_is_validated")
                .OnTable("certificates")
                .OnColumn("is_validated");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_certificates_is_created")
                .OnTable("certificates")
                .OnColumn("is_created");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_certificates_expires_at")
                .OnTable("certificates")
                .OnColumn("expires_at");
        }
	}
}