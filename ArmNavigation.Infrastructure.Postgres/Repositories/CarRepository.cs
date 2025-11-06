using System.Data;
using ArnNavigation.Application.Repositories;
using ArmNavigation.Domain.Models;
using Dapper;
using Npgsql;

namespace ArmNavigation.Infrastructure.Repositories
{
    public sealed class CarRepository : BaseRepository, ICarRepository
    {
        public async Task<IEnumerable<Car>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken ct)
        {
            var sql = """
            SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
            FROM "Cars" c
            WHERE c."IsRemoved" = false
            """;

            object? param = null;
            if (medInstitutionId.HasValue)
            {
                sql += " AND c.\"MedInstitutionId\" = @orgId";
                param = new { orgId = medInstitutionId.Value };
            }

            return await ExecuteQueryAsync<Car>(sql, param, ct);
        }

        public async Task<Car?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
            SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
            FROM "Cars" c
            WHERE c."CarId" = @id AND c."IsRemoved" = false
            """;
            return await ExecuteQuerySingleOrDefaultAsync<Car>(sql, new { id }, ct);
        }

        public async Task<Guid> CreateAsync(Car car, CancellationToken ct)
        {
            var id = car.CarId != Guid.Empty ? car.CarId : Guid.NewGuid();
            const string sql = """
            INSERT INTO "Cars" ("CarId", "RegNum", "Gps-tracker", "IsRemoved", "MedInstitutionId")
            VALUES (@id, @reg, @gps, false, @orgId)
            RETURNING "CarId"
            """;

            return await ExecuteScalarAsync<Guid>(sql, new
            {
                id,
                reg = car.RegNum,
                gps = car.GpsTracker,
                orgId = car.MedInstitutionId
            }, ct);
        }

        public async Task UpdateAsync(Car car, CancellationToken ct)
        {
            const string sql = """
            UPDATE "Cars"
            SET "RegNum" = @reg, "Gps-tracker" = @gps, "MedInstitutionId" = @orgId
            WHERE "CarId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new
            {
                id = car.CarId,
                reg = car.RegNum,
                gps = car.GpsTracker,
                orgId = car.MedInstitutionId
            }, ct);
        }

        public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
            UPDATE "Cars"
            SET "IsRemoved" = true
            WHERE "CarId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new { id }, ct);
        }

        public async Task<IEnumerable<Car>> GetAsync(string query, Guid? medInstitutionId, CancellationToken ct)
        {
            var sql = """
                SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
                FROM "Cars" c
                WHERE c."IsRemoved" = false
                  AND (c."RegNum" ILIKE @q OR c."Gps-tracker" ILIKE @q)
                """;

            object param = new { q = $"%{query}%" };

            if (medInstitutionId.HasValue)
            {
                sql += " AND c.\"MedInstitutionId\" = @orgId";
                param = new { q = $"%{query}%", orgId = medInstitutionId.Value };
            }

            return await ExecuteQueryAsync<Car>(sql, param, ct);
        }

        public async Task BindTrackerAsync(Guid carId, string tracker, CancellationToken ct)
        {
            const string sql = """
            UPDATE "Cars"
            SET "Gps-tracker" = @gps
            WHERE "CarId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new { id = carId, gps = tracker }, ct);
        }

        public async Task UnbindTrackerAsync(Guid carId, CancellationToken ct)
        {
            const string sql = """
            UPDATE "Cars"
            SET "Gps-tracker" = NULL
            WHERE "CarId" = @id AND "IsRemoved" = false
            """;

            await ExecuteNonQueryAsync(sql, new { id = carId }, ct);
        }
    }
}