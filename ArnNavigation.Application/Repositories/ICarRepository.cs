using ArmNavigation.Domain.Models;

namespace ArnNavigation.Application.Repositories
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllByOrgAsync(Guid? medInstitutionId, CancellationToken token);
        Task<Car?> GetByIdAsync(Guid id, CancellationToken token);
        Task<Car?> GetByRegNumAsync(string regNum, CancellationToken token);
        Task<Guid> CreateAsync(Car car, CancellationToken token);
        Task<Car> UpdateAsync(Car car, CancellationToken token);
        Task<Car> SoftDeleteAsync(Guid id, CancellationToken token);
        Task<IEnumerable<Car>> GetAsync(string query, Guid? medInstitutionId, CancellationToken token);
        Task<Car> BindTrackerAsync(Guid carId, string tracker, CancellationToken token);
        Task<Car> UnbindTrackerAsync(Guid carId, CancellationToken token);
    }
}




