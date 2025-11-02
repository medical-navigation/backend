using FluentMigrator;

namespace ArmNavigation.Infrastructure.Migrator.Migrations
{
    [Migration(2025110201)]
    public class CreateSchema : Migration
    {
        public override void Up()
        {
            Execute.Sql("""
                    CREATE SCHEMA IF NOT EXISTS arm_navigation_schema;
                    """);
        }

        public override void Down()
        {
            Execute.Sql("""
                    DROP SCHEMA IF EXISTS arm_navigation_schema;
                    """);
        }
    }
}
