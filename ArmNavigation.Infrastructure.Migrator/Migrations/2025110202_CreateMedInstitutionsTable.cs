using FluentMigrator;

namespace ArmNavigation.Infrastructure.Migrator.Migrations
{
    [Migration(2025110202)]
    public class CreateMedInstitutionsTable : Migration
    {
        public override void Up()
        {
            Execute.Sql("""
                     CREATE TABLE "MedInstitutions" ("MedInstitutionId" UUID PRIMARY KEY,
                     "Name" text not null UNIQUE,
                     "IsRemoved" bool default false not null);
                     """);
        }

        public override void Down()
        {
            Execute.Sql("""
                    DROP TABLE "MedInstitutions"
                    """);
        }
    }
}
