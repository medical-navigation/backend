using ArmNavigation.Domain.Models;

namespace ArnNavigation.Application.Services
{
    public interface IPositionService
    {
        Task<Guid> SavePositionAsync(Guid carId, DateTime time, Point coordinates, CancellationToken token);
        Task<Position?> GetLastPositionAsync(Guid carId, CancellationToken token);
        Task ProcessPositionAsync(PositionMessage message, CancellationToken token);
    }
}
