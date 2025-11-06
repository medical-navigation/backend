using ArnNavigation.Application.Repositories;
using ArmNavigation.Domain.Models;
using Dapper;

namespace ArmNavigation.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository, IUserRepository
{
    private sealed class DbUserRow
    {
        public Guid UserId { get; init; }
        public string Login { get; init; } = default!;
        public string PasswordHash { get; init; } = default!;
        public Guid MedInstitutionId { get; init; }
        public bool IsRemoved { get; init; }
        public int Role { get; init; }
    }

    private static User MapToUser(DbUserRow row) => new()
    {
        UserId = row.UserId,
        Login = row.Login,
        PasswordHash = row.PasswordHash,
        MedInstitutionId = row.MedInstitutionId,
        Role = row.Role,
        IsRemoved = row.IsRemoved
    };

    public async Task<User?> GetByLoginAsync(string login, CancellationToken ct)
    {
        const string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."Login" = @login AND u."IsRemoved" = false
            """;

        var row = await ExecuteQuerySingleOrDefaultAsync<DbUserRow>(sql, new { login }, ct);
        return row is null ? null : MapToUser(row);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."UserId" = @id AND u."IsRemoved" = false
            """;

        var row = await ExecuteQuerySingleOrDefaultAsync<DbUserRow>(sql, new { id }, ct);
        return row is null ? null : MapToUser(row);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken ct)
    {
        var id = user.UserId != Guid.Empty ? user.UserId : Guid.NewGuid();
        const string sql = """
            INSERT INTO "Users" ("UserId", "Login", "Password", "IsRemoved", "Role", "MedInstitutionId")
            VALUES (@id, @login, @password, false, @role, @orgId)
            RETURNING "UserId"
            """;

        return await ExecuteScalarAsync<Guid>(sql, new
        {
            id,
            login = user.Login,
            password = user.PasswordHash,
            role = user.Role,
            orgId = user.MedInstitutionId
        }, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        const string sql = """
            UPDATE "Users"
            SET "Login" = @login, "Password" = @password, "Role" = @role, "MedInstitutionId" = @orgId
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

        await ExecuteNonQueryAsync(sql, new
        {
            id = user.UserId,
            login = user.Login,
            password = user.PasswordHash,
            role = user.Role,
            orgId = user.MedInstitutionId
        }, ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        const string sql = """
            UPDATE "Users"
            SET "IsRemoved" = true
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

        await ExecuteNonQueryAsync(sql, new { id }, ct);
    }

    public async Task<IEnumerable<User>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken ct)
    {
        var sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."IsRemoved" = false
            """;

        object? param = null;
        if (medInstitutionId.HasValue)
        {
            sql += " AND u.\"MedInstitutionId\" = @orgId";
            param = new { orgId = medInstitutionId.Value };
        }

        var rows = await ExecuteQueryAsync<DbUserRow>(sql, param, ct);
        return rows.Select(MapToUser);
    }
}