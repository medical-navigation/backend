using ArmNavigation.Domain.Models;
using ArmNavigation.Presentation.Hubs;
using ArmNavigation.Presentation.ResponseModels;
using ArnNavigation.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace ArmNavigation.Presentation.Services
{
    public class SignalRPositionNotifier : IPositionNotifier
    {
        private readonly IHubContext<PositionHub> _hubContext;

        public SignalRPositionNotifier(IHubContext<PositionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyPositionAsync(PositionMessage message, CancellationToken token = default)
        {
            var positionData = new PositionUpdateDto
            {
                SsmpId = message.SsmpId,
                CarNumber = message.CarNumber,
                CarId = message.CarId,
                DeviceId = message.DeviceId,
                Latitude = message.Latitude,
                Longitude = message.Longitude,
                Timestamp = message.Timestamp
            };

            // Отправляем всем подключенным клиентам
            await _hubContext.Clients.All.SendAsync("ReceivePosition", positionData, token);

            // Также отправляем в группу конкретной машины (если клиенты подписаны)
            await _hubContext.Clients.Group($"car-{message.CarNumber}")
                .SendAsync("ReceivePosition", positionData, token);
        }
    }
}