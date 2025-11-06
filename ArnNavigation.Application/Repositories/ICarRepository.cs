using ArmNavigation.Domain.Models;

namespace ArnNavigation.Application.Repositories
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken ct);
        Task<Car?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Guid> CreateAsync(Car car, CancellationToken ct);
        Task UpdateAsync(Car car, CancellationToken ct);
        Task SoftDeleteAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Car>> GetAsync(string query, Guid? medInstitutionId, CancellationToken ct);
        Task BindTrackerAsync(Guid carId, string tracker, CancellationToken ct);
        Task UnbindTrackerAsync(Guid carId, CancellationToken ct);
    }
}




