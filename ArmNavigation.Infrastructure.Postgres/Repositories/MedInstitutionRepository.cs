using ArnNavigation.Application.Repositories;
using ArmNavigation.Domain.Models;
using Dapper;

namespace ArmNavigation.Infrastructure.Repositories
{
    public sealed class MedInstitutionRepository : BaseRepository, IMedInstitutionRepository
    {
        public async Task<IEnumerable<MedInstitution>> GetAllAsync(CancellationToken ct)
        {
            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "IsRemoved" = false
            """;
            return await ExecuteQueryAsync<MedInstitution>(sql, ct: ct);
        }

        public async Task<IEnumerable<MedInstitution>> GetAllByNameAsync(string? nameFilter, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(nameFilter))
                return await GetAllAsync(ct);

            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "IsRemoved" = false AND "Name" ILIKE @name
            """;

            return await ExecuteQueryAsync<MedInstitution>(sql, new { name = $"%{nameFilter}%" }, ct);
        }

        public async Task<MedInstitution?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
            SELECT "MedInstitutionId", "Name", "IsRemoved"
            FROM "MedInstitutions"
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;
            return await ExecuteQuerySingleOrDefaultAsync<MedInstitution>(sql, new { id }, ct);
        }

        public async Task<Guid> CreateAsync(MedInstitution medInstitution, CancellationToken ct)
        {
            var id = medInstitution.MedInstitutionId != Guid.Empty ? medInstitution.MedInstitutionId : Guid.NewGuid();
            const string sql = """
            INSERT INTO "MedInstitutions" ("MedInstitutionId", "Name", "IsRemoved")
            VALUES (@id, @name, false)
            RETURNING "MedInstitutionId"
            """;

            return await ExecuteScalarAsync<Guid>(sql, new { id, name = medInstitution.Name }, ct);
        }

        public async Task UpdateAsync(MedInstitution medInstitution, CancellationToken ct)
        {
            const string sql = """
            UPDATE "MedInstitutions"
            SET "Name" = @name
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new { id = medInstitution.MedInstitutionId, name = medInstitution.Name }, ct);
        }

        public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
            UPDATE "MedInstitutions"
            SET "IsRemoved" = true
            WHERE "MedInstitutionId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new { id }, ct);
        }
    }
}
