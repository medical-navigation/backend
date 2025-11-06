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

    public async Task<User?> GetByLoginAsync(string login, CancellationToken ct)
    {
        const string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."Login" = @login AND u."IsRemoved" = false
            """;

        var parameters = new DynamicParameters();
        parameters.Add("login", login);

        var row = await QuerySingleOrDefaultAsync<DbUserRow>(sql, parameters, ct);
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

        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        var row = await QuerySingleOrDefaultAsync<DbUserRow>(sql, parameters, ct);
        return row is null ? null : MapToUser(row);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken ct)
    {
        var id = user.UserId != Guid.Empty ? user.UserId : Guid.NewGuid();
        const string sql = """
            INSERT INTO "Users" ("UserId", "Login", "Password", "IsRemoved", "Role", "MedInstitutionId")
            VALUES (@id, @login, @password, false, @role, @orgId)
            """;

        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("login", user.Login);
        parameters.Add("password", user.PasswordHash);
        parameters.Add("role", user.Role);
        parameters.Add("orgId", user.MedInstitutionId);

        await ExecuteAsync(sql, parameters, ct);
        return id;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct)
    {
        const string sql = """
            UPDATE "Users"
            SET "Login" = @login, "Password" = @password,
                "Role" = @role,
                "MedInstitutionId" = @orgId
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

        var parameters = new DynamicParameters();
        parameters.Add("id", user.UserId);
        parameters.Add("login", user.Login);
        parameters.Add("password", user.PasswordHash);
        parameters.Add("role", user.Role);
        parameters.Add("orgId", user.MedInstitutionId);

        var affected = await ExecuteAsync(sql, parameters, ct);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        const string sql = """
            UPDATE "Users"
            SET "IsRemoved" = true
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        var affected = await ExecuteAsync(sql, parameters, ct);
        return affected > 0;
    }

    public async Task<IEnumerable<User>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken ct)
    {
        var where = medInstitutionId.HasValue ? "AND u.\"MedInstitutionId\" = @orgId" : string.Empty;
        var sql = $"""
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."IsRemoved" = false {where}
            """;

        var parameters = new DynamicParameters();
        if (medInstitutionId.HasValue)
            parameters.Add("orgId", medInstitutionId.Value);

        var rows = await QueryAsync<DbUserRow>(sql, parameters, ct);
        return rows.Select(MapToUser);
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
}