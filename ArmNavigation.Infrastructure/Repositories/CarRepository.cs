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
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate("""
                SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
                FROM "Cars" c
                /**where**/
                """);

            sqlBuilder.Where("c.\"IsRemoved\" = false");

            if (medInstitutionId.HasValue)
            {
                sqlBuilder.Where("c.\"MedInstitutionId\" = @orgId");
            }

            var parameters = new DynamicParameters();
            if (medInstitutionId.HasValue)
                parameters.Add("orgId", medInstitutionId.Value);

            var result = await QueryAsync<Car>(template.RawSql, parameters, ct);
            return result;
        }

        public async Task<Car?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
                SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
                FROM "Cars" c
                WHERE c."CarId" = @id AND c."IsRemoved" = false
                """;

            var parameters = new DynamicParameters();
            parameters.Add("id", id);

            return await QuerySingleOrDefaultAsync<Car>(sql, parameters, ct);
        }

        public async Task<Guid> CreateAsync(Car car, CancellationToken ct)
        {
            var id = car.CarId != Guid.Empty ? car.CarId : Guid.NewGuid();
            const string sql = """
                INSERT INTO "Cars" ("CarId", "RegNum", "Gps-tracker", "IsRemoved", "MedInstitutionId")
                VALUES (@id, @regNum, @gpsTracker, false, @medInstitutionId)
                """;

            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            parameters.Add("regNum", car.RegNum);
            parameters.Add("gpsTracker", car.GpsTracker);
            parameters.Add("medInstitutionId", car.MedInstitutionId);

            await ExecuteAsync(sql, parameters, ct);
            return id;
        }

        public async Task<bool> UpdateAsync(Car car, CancellationToken ct)
        {
            const string sql = """
                UPDATE "Cars"
                SET "RegNum" = @regNum, "Gps-tracker" = @gpsTracker, "MedInstitutionId" = @medInstitutionId
                WHERE "CarId" = @carId AND "IsRemoved" = false
                """;

            var parameters = new DynamicParameters();
            parameters.Add("carId", car.CarId);
            parameters.Add("regNum", car.RegNum);
            parameters.Add("gpsTracker", car.GpsTracker);
            parameters.Add("medInstitutionId", car.MedInstitutionId);

            int affectedRows = await ExecuteAsync(sql, parameters, ct);
            bool wasUpdated = affectedRows > 0;
            return wasUpdated;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
        {
            const string sql = """
                UPDATE "Cars"
                SET "IsRemoved" = true
                WHERE "CarId" = @carId AND "IsRemoved" = false
                """;

            var parameters = new DynamicParameters();
            parameters.Add("carId", id);

            int affectedRows = await ExecuteAsync(sql, parameters, ct);
            bool wasDeleted = affectedRows > 0;
            return wasDeleted;
        }

        public async Task<IEnumerable<Car>> GetAsync(string query, Guid? medInstitutionId, CancellationToken ct)
        {
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate("""
                SELECT c."CarId", c."RegNum", c."MedInstitutionId", c."Gps-tracker" AS "GpsTracker", c."IsRemoved"
                FROM "Cars" c
                /**where**/
                """);

            sqlBuilder.Where("c.\"IsRemoved\" = false");
            sqlBuilder.Where("(c.\"RegNum\" ILIKE @searchQuery OR c.\"Gps-tracker\" ILIKE @searchQuery)");

            if (medInstitutionId.HasValue)
            {
                sqlBuilder.Where("c.\"MedInstitutionId\" = @medInstitutionId");
            }

            var parameters = new DynamicParameters();
            parameters.Add("searchQuery", $"%{query}%");
            if (medInstitutionId.HasValue)
                parameters.Add("medInstitutionId", medInstitutionId.Value);

            return await QueryAsync<Car>(template.RawSql, parameters, ct);
        }

        public async Task<bool> BindTrackerAsync(Guid carId, string tracker, CancellationToken ct)
        {
            const string sql = """
                UPDATE "Cars"
                SET "Gps-tracker" = @gpsTracker
                WHERE "CarId" = @carId AND "IsRemoved" = false
                """;

            var parameters = new DynamicParameters();
            parameters.Add("carId", carId);
            parameters.Add("gpsTracker", tracker);

            int affectedRows = await ExecuteAsync(sql, parameters, ct);
            bool wasUpdated = affectedRows > 0;
            return wasUpdated;
        }

        public async Task<bool> UnbindTrackerAsync(Guid carId, CancellationToken ct)
        {
            const string sql = """
                UPDATE "Cars"
                SET "Gps-tracker" = NULL
                WHERE "CarId" = @carId AND "IsRemoved" = false
                """;

            var parameters = new DynamicParameters();
            parameters.Add("carId", carId);

            int affectedRows = await ExecuteAsync(sql, parameters, ct);
            bool wasUpdated = affectedRows > 0;
            return wasUpdated;
        }
    }
}