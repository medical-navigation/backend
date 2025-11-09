using Microsoft.AspNetCore.SignalR;

namespace ArmNavigation.Presentation.Hubs
{
    public class PositionHub : Hub
    {
        // Метод для отправки позиции всем подключенным клиентам
        public async Task SendPosition(object position)
        {
            await Clients.All.SendAsync("ReceivePosition", position);
        }

        // Метод для подписки на обновления конкретной машины
        public async Task SubscribeToCarUpdates(string carNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"car-{carNumber}");
        }

        // Метод для отписки от обновлений конкретной машины
        public async Task UnsubscribeFromCarUpdates(string carNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"car-{carNumber}");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}