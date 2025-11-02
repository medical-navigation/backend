using FluentMigrator;

namespace ArmNavigation.Infrastructure.Migrator.Migrations
{
    [Migration(2025110203)]
    public class CreateUserRolesTable : Migration
    {
        public override void Up()
        {
            Execute.Sql("""
                     CREATE TABLE "UserRoles" ("UserRoleId" UUID PRIMARY KEY,
                     "Name" text not null UNIQUE,
                     "IsRemoved" bool default false not null);
                     """);
        }

        public override void Down()
        {
            Execute.Sql("""
                    DROP TABLE "UserRoles"
                    """);
        }
    }
}
