using System;
using FluentMigrator;

namespace DeployTools.Core.DataAccess.Migrations
{
    public class BaseMigration : AutoReversingMigration
    {
        public override void Up()
        {
            throw new NotImplementedException();
        }

        public FluentMigrator.Builders.Create.Table.ICreateTableColumnOptionOrWithColumnSyntax
            CreateBaseColumnsAndIndexes(string tableName, int keyLength = 32)
        {
            var o = CreateBaseColumns(tableName, keyLength);
            CreateBaseIndexes(tableName);
            return o;
        }

        public FluentMigrator.Builders.Create.Table.ICreateTableColumnOptionOrWithColumnSyntax CreateBaseColumns(string tableName, int keyLength)
        {
            return IfDatabase("PostgreSQL")
                .Create
                .Table(tableName)
                .WithColumn("id").AsString(keyLength).NotNullable()
                .WithColumn("created_at").AsCustom("timestamp with time zone").NotNullable()
                .WithColumn("updated_at").AsCustom("timestamp with time zone").Nullable();
        }

        public void CreateBaseIndexes(string tableName)
        {
            IfDatabase("PostgreSQL")
                .Create
                .PrimaryKey($"pk_{tableName}")
                .OnTable(tableName)
                .Column("id");

            IfDatabase("PostgreSQL")
                .Create
                .Index($"ix_{tableName}_created_at")
                .OnTable(tableName)
                .OnColumn("created_at");

            IfDatabase("PostgreSQL")
                .Create
                .Index($"ix_{tableName}_updated_at")
                .OnTable(tableName)
                .OnColumn("updated_at");
        }
    }
}
