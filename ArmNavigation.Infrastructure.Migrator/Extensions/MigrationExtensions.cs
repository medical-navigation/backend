using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;

namespace ArmNavigation.Infrastructure.Migrator.Extensions;

public static class MigrationExtensions
{
    public static void AddMigration(this IServiceCollection services,
        string connectionString)
    {
        services.AddLogging(c => c.AddFluentMigratorConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(
                x => x.AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(Assembly.GetExecutingAssembly())
                    .For.Migrations());
    }
}