using ArmNavigation.Domain.Models;

namespace ArnNavigation.Application.Repositories
{
    public interface IPositionRepository
    {
        Task<Guid> CreateAsync(Position position, CancellationToken token);
        Task<Position?> GetLastPositionAsync(Guid carId, CancellationToken token);
    }
}
