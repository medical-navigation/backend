using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace ArnNavigation.Application.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly ICarRepository _carRepository;
        private readonly IPositionNotifier _positionNotifier;
        private readonly ILogger<PositionService> _logger;

        public PositionService(
            IPositionRepository positionRepository,
            ICarRepository carRepository,
            IPositionNotifier positionNotifier,
            ILogger<PositionService> logger)
        {
            _positionRepository = positionRepository;
            _carRepository = carRepository;
            _positionNotifier = positionNotifier;
            _logger = logger;
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

            if (string.IsNullOrWhiteSpace(message.CarNumber))
                throw new ArgumentException("CarNumber не может быть пустым", nameof(message.CarNumber));

            // Находим машину по номеру
            var car = await _carRepository.GetByRegNumAsync(message.CarNumber, token);
            if (car == null)
            {
                _logger.LogWarning("Машина с номером {CarNumber} не найдена в базе данных", message.CarNumber);
                return;
            }

            message.CarId = car.CarId;

            // Сохраняем позицию в БД
            var coordinates = new Point(message.Longitude, message.Latitude);
            await SavePositionAsync(car.CarId, message.Timestamp, coordinates, token);

            _logger.LogInformation(
                "Позиция сохранена для машины {CarNumber} (ID: {CarId}): Lat={Lat}, Lon={Lon}",
                message.CarNumber, car.CarId, message.Latitude, message.Longitude);

            // Отправляем уведомление фронту (через SignalR или другой механизм)
            await _positionNotifier.NotifyPositionAsync(message, token);

            _logger.LogInformation("Уведомление о позиции отправлено фронту");
        }
    }
}
