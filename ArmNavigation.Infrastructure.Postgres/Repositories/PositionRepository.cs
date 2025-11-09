using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Repositories;

namespace ArmNavigation.Infrastructure.Postgres.Repositories
{
    public sealed class PositionRepository : BaseRepository, IPositionRepository
    {
        public async Task<Guid> CreateAsync(Position position, CancellationToken token)
        {
            const string sql = """
                INSERT INTO "Positions" ("PositionId", "Time", "Coordinates", "CarId")
                VALUES (@id, @time, point(@lat, @lng), @carId)
                RETURNING "PositionId"
                """;

            return await ExecuteScalarAsync<Guid>(sql, new
            {
                id = position.PositionId,
                time = position.Timestamp,
                lat = position.Coordinates.Latitude,
                lng = position.Coordinates.Longitude,
                carId = position.CarId
            }, token);
        }

        public async Task<Position?> GetLastPositionAsync(Guid carId, CancellationToken token)
        {
            const string sql = """
                SELECT "PositionId", "Time" as "Timestamp", "Coordinates", "CarId"
                FROM "Positions"
                WHERE "CarId" = @carId
                ORDER BY "Time" DESC
                LIMIT 1
                """;

            var result = await ExecuteQuerySingleOrDefaultAsync<Position>(sql, new { carId }, token);
            return result;
        }
    }
}
