using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace ArmNavigation.Infrastructure.RabbitMQ.Consumers
{
    public class PositionConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PositionConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        private const string QueueName = "position-queue";

        public PositionConsumer(IServiceProvider serviceProvider, ILogger<PositionConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel!);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Получено сообщение:\n{Message}", message);

                    // Разбор XML
                    var xml = XDocument.Parse(message);
                    var record = xml.Root?.Element("RECORD");
                    if (record == null)
                    {
                        _logger.LogWarning("XML не содержит элемент <RECORD>.");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // Извлечение атрибутов RECORD
                    var carId = record.Attribute("CAR_ID")?.Value;
                    var latStr = record.Attribute("LATITUDE")?.Value;
                    var lonStr = record.Attribute("LONGITUDE")?.Value;
                    var dateStr = record.Attribute("DATETIME")?.Value;

                    if (string.IsNullOrEmpty(latStr) || string.IsNullOrEmpty(lonStr))
                    {
                        _logger.LogWarning("LATITUDE или LONGITUDE отсутствуют в RECORD.");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
                    {
                        _logger.LogWarning("Не удалось разобрать LATITUDE = {Lat}", latStr);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (!double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                    {
                        _logger.LogWarning("Не удалось разобрать LONGITUDE = {Lon}", lonStr);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    var timestamp = DateTime.TryParse(dateStr, out var parsedDate)
                        ? parsedDate
                        : DateTime.UtcNow;

                    _logger.LogInformation("RECORD: Car={CarId}, Lat={Lat}, Lon={Lon}, Time={Time}",
                        carId, latitude, longitude, timestamp);

                    using var scope = _serviceProvider.CreateScope();
                    var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();

                    var coordinates = new Point(longitude, latitude);
                    await positionService.SavePositionAsync(Guid.NewGuid(), timestamp, coordinates, stoppingToken);

                    _logger.LogInformation("Позиция сохранена в БД");

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения из RabbitMQ");
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
