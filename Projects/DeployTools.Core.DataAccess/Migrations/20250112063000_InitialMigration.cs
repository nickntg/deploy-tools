using FluentMigrator;
using DeployTools.Core.DataAccess.Migrations;

namespace AccountsApi.Core.DataAccess.Migrations
{
	[Migration(202501120630)]
	public class _20250112063000_InitialMigration : BaseMigration
	{
		public override void Up()
		{
			CreateBaseColumnsAndIndexes("hosts")
                .WithColumn("address").AsString(256).NotNullable()
                .WithColumn("ssh_user_name").AsString(256).NotNullable()
                .WithColumn("key_file").AsString().NotNullable()
                .WithColumn("next_free_port").AsInt32().NotNullable();

            CreateBaseColumnsAndIndexes("packages")
                .WithColumn("deployable_location").AsString().NotNullable()
                .WithColumn("executable_file").AsString().NotNullable()
                .WithColumn("name").AsString(256).NotNullable();

            CreateBaseColumnsAndIndexes("applications")
                .WithColumn("host_id").AsString(32).NotNullable()
                .WithColumn("package_id").AsString(32).NotNullable()
                .WithColumn("name").AsString().NotNullable()
                .WithColumn("port").AsInt32().NotNullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_applications_host_id")
                .OnTable("applications")
                .OnColumn("host_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_applications_package_id")
                .OnTable("applications")
                .OnColumn("package_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_applications_name")
                .OnTable("applications")
                .OnColumn("name")
                .Unique();

            CreateBaseColumnsAndIndexes("application_deploys")
                .WithColumn("host_id").AsString(32).NotNullable()
                .WithColumn("application_id").AsString(32).NotNullable()
                .WithColumn("is_successful").AsBoolean().NotNullable()
                .WithColumn("is_completed").AsBoolean().NotNullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deploys_application_id")
                .OnTable("application_deploys")
                .OnColumn("application_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deploys_host_id")
                .OnTable("application_deploys")
                .OnColumn("host_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_application_deploys_is_completed")
                .OnTable("application_deploys")
                .OnColumn("is_completed");

            CreateBaseColumnsAndIndexes("journal_entries")
                .WithColumn("command_completed").AsCustom("timestamp with time zone").NotNullable()
                .WithColumn("command_started").AsCustom("timestamp with time zone").NotNullable()
                .WithColumn("command_executed").AsString().NotNullable()
                .WithColumn("deploy_id").AsString(32).NotNullable()
                .WithColumn("output").AsString().Nullable()
                .WithColumn("was_successful").AsBoolean().NotNullable();

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_journal_entries_deploy_id")
                .OnTable("journal_entries")
                .OnColumn("deploy_id");

            IfDatabase("PostgreSQL")
                .Create
                .Index("ix_journal_entries_was_successful")
                .OnTable("journal_entries")
                .OnColumn("was_successful");
        }
	}
}