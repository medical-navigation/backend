using ArmNavigation.Domain.Models;

namespace ArnNavigation.Application.Services
{
    /// <summary>
    /// Интерфейс для отправки уведомлений о позициях (например, через SignalR)
    /// </summary>
    public interface IPositionNotifier
    {
        Task NotifyPositionAsync(PositionMessage message, CancellationToken token = default);
    }
}