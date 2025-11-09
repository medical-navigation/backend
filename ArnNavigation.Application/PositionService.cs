using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Repositories;

namespace ArnNavigation.Application.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;

        public PositionService(IPositionRepository positionRepository)
        {
            _positionRepository = positionRepository;
        }

        public async Task<Guid> SavePositionAsync(Guid carId, DateTime time, Point coordinates, CancellationToken token)
        {
            var position = new Position
            {
                PositionId = Guid.NewGuid(),
                CarId = carId,
                Timestamp = time,
                Coordinates = coordinates
            };

            return await _positionRepository.CreateAsync(position, token);
        }

        public async Task<Position?> GetLastPositionAsync(Guid carId, CancellationToken token)
        {
            return await _positionRepository.GetLastPositionAsync(carId, token);
        }

        public async Task ProcessPositionAsync(PositionMessage message, CancellationToken token)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.CarId == Guid.Empty)
                throw new ArgumentException("CarId не может быть пустым", nameof(message.CarId));

            var coordinates = new Point(message.Latitude, message.Longitude);

            await SavePositionAsync(message.CarId, message.Timestamp, coordinates, token);
        }
    }
}
