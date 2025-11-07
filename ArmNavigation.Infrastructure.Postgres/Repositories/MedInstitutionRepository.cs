using ArnNavigation.Application.Repositories;
using ArmNavigation.Domain.Models;
using Dapper;

namespace ArmNavigation.Infrastructure.Postgres.Repositories
{
    public sealed class MedInstitutionRepository : BaseRepository, IMedInstitutionRepository
    {
        public async Task<IEnumerable<MedInstitution>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "IsRemoved" = false
            """;
            return await ExecuteQueryAsync<MedInstitution>(sql, cancellationToken);
        }

        public async Task<IEnumerable<MedInstitution>> GetAllByNameAsync(string? nameFilter, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nameFilter))
                return await GetAllAsync(cancellationToken);

            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "IsRemoved" = false AND "Name" ILIKE @name
            """;

            return await ExecuteQueryAsync<MedInstitution>(sql, new { name = $"%{nameFilter}%" }, cancellationToken);
        }

        public async Task<MedInstitution?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;
            return await ExecuteQuerySingleOrDefaultAsync<MedInstitution>(sql, new { id }, cancellationToken);
        }

        public async Task<Guid> CreateAsync(MedInstitution medInstitution, CancellationToken cancellationToken)
        {
            var id = medInstitution.MedInstitutionId != Guid.Empty ? medInstitution.MedInstitutionId : Guid.NewGuid();
            const string sql = """
            INSERT INTO "MedInstitutions" ("MedInstitutionId", "Name", "IsRemoved")
            VALUES (@id, @name, false)
            RETURNING "MedInstitutionId"
            """;

            return await ExecuteScalarAsync<Guid>(sql, new { id, name = medInstitution.Name }, cancellationToken);
        }

        public async Task<MedInstitution> UpdateAsync(MedInstitution medInstitution, CancellationToken cancellationToken)
        {
            const string sql = """
            UPDATE "MedInstitutions"
            SET "Name" = @name
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;

            return await ExecuteNonQueryAsync<MedInstitution>(sql, new { id = medInstitution.MedInstitutionId, name = medInstitution.Name }, cancellationToken);
        }

        public async Task<MedInstitution> SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = """
            UPDATE "MedInstitutions"
            SET "IsRemoved" = true
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;

            return await ExecuteNonQueryAsync<MedInstitution>(sql, new { id }, cancellationToken);
        }
    }
}
