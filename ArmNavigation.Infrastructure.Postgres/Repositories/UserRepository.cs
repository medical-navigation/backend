using ArnNavigation.Application.Repositories;
using ArmNavigation.Domain.Models;
using Dapper;
using ArmNavigation.Infrastructure.Postgres.Mappers;

namespace ArmNavigation.Infrastructure.Postgres.Repositories
{
    public sealed class UserRepository : BaseRepository, IUserRepository
    {
        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."Login" = @login AND u."IsRemoved" = false
            """;

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("login", login);
            var row = await ExecuteQuerySingleOrDefaultAsync<User>(sql, parameters, cancellationToken);
            return row is null ? null : UserMapper.MapToUser(row);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."UserId" = @id AND u."IsRemoved" = false
            """;

            var row = await ExecuteQuerySingleOrDefaultAsync<User>(sql, new { id }, cancellationToken);
            return row is null ? null : UserMapper.MapToUser(row);
        }

        public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken)
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
            }, cancellationToken);
        }

        public async Task<User> UpdateAsync(User user, CancellationToken token)
        {
            const string sql = """
            UPDATE "Users"
            SET "Login" = @login, "Password" = @password, "Role" = @role, "MedInstitutionId" = @orgId
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

            return await ExecuteNonQueryAsync<User>(sql, new
            {
                id = user.UserId,
                login = user.Login,
                password = user.PasswordHash,
                role = user.Role,
                orgId = user.MedInstitutionId
            }, token);
        }

        public async Task<User> SoftDeleteAsync(Guid id, CancellationToken token)
        {
            const string sql = """
            UPDATE "Users"
            SET "IsRemoved" = true
            WHERE "UserId" = @id AND "IsRemoved" = false
            """;

            return await ExecuteNonQueryAsync<User>(sql, new { id }, token);
        }

        public async Task<IEnumerable<User>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken token)
        {
            string sql = """
            SELECT u."UserId", u."Login", u."Password" AS "PasswordHash", u."MedInstitutionId",
                   u."IsRemoved", u."Role"
            FROM "Users" u
            WHERE u."IsRemoved" = false
            """;

            var parameters = new DynamicParameters();
            if (medInstitutionId.HasValue)
                parameters.Add("orgId", medInstitutionId.Value);

            return await ExecuteQueryAsync<User>(sql, parameters, token);
        }
    }
}