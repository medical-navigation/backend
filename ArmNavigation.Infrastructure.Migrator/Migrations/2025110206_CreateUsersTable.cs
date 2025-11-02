using FluentMigrator;

namespace ArmNavigation.Infrastructure.Migrator.Migrations
{
    [Migration(2025110206)]
    public class CreateUsersTable : Migration
    {
        public override void Up()
        {
            // TODO медорганизации не удаляются каскадно, дописать это в табличку с медорганизацией
            Execute.Sql("""
                     CREATE TABLE "Users" ("UserId" UUID PRIMARY KEY,
                     "Login" text not null UNIQUE,
                     "Password" text not null,
                     "IsRemoved" bool default false not null,
                     "UserRoleId" UUID,
                     "MedInstitutionId" UUID not null,

                     CONSTRAINT fk_UserRoles_Users FOREIGN KEY ("UserRoleId") REFERENCES "UserRoles" ("UserRoleId") ON DELETE CASCADE,
                     CONSTRAINT fk_MedInstitutions_Users FOREIGN KEY ("MedInstitutionId") REFERENCES "MedInstitutions" ("MedInstitutionId"));
                     """);
        }

        public override void Down()
        {
            Execute.Sql("""
                    DROP TABLE "Users"
                    """);
        }
    }
}
