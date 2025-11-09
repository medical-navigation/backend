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
        }

        private async Task InitializeRabbitMQAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            const int maxRetries = 10;
            const int delayMs = 3000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.LogInformation("Попытка подключения к RabbitMQ ({Attempt}/{MaxRetries})...", i + 1, maxRetries);
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
                    _logger.LogInformation("Успешно подключились к RabbitMQ");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось подключиться к RabbitMQ. Попытка {Attempt}/{MaxRetries}", i + 1, maxRetries);

                    if (i == maxRetries - 1)
                    {
                        _logger.LogError("Не удалось подключиться к RabbitMQ после {MaxRetries} попыток", maxRetries);
                        throw;
                    }

                    await Task.Delay(delayMs, cancellationToken);
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMQAsync(stoppingToken);

            var consumer = new EventingBasicConsumer(_channel!);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var xmlMessage = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Получено сообщение из RabbitMQ:\n{Message}", xmlMessage);

                    // Разбор XML
                    var xml = XDocument.Parse(xmlMessage);
                    var record = xml.Root?.Element("RECORD");
                    if (record == null)
                    {
                        _logger.LogWarning("XML не содержит элемент <RECORD>");
                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // Извлечение необходимых атрибутов
                    var ssmpId = record.Attribute("SSMP_ID")?.Value ?? string.Empty;
                    var carNumber = record.Attribute("CAR_ID")?.Value ?? string.Empty;
                    var deviceId = record.Attribute("DEVICE_ID")?.Value ?? string.Empty;
                    var latStr = record.Attribute("LATITUDE")?.Value;
                    var lonStr = record.Attribute("LONGITUDE")?.Value;
                    var dateStr = record.Attribute("DATETIME")?.Value;

                    // Валидация обязательных полей
                    if (string.IsNullOrWhiteSpace(carNumber))
                    {
                        _logger.LogWarning("CAR_ID отсутствует в RECORD");
                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (string.IsNullOrEmpty(latStr) || string.IsNullOrEmpty(lonStr))
                    {
                        _logger.LogWarning("LATITUDE или LONGITUDE отсутствуют в RECORD");
                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // Парсинг координат
                    if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
                    {
                        _logger.LogWarning("Не удалось разобрать LATITUDE = {Lat}", latStr);
                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (!double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                    {
                        _logger.LogWarning("Не удалось разобрать LONGITUDE = {Lon}", lonStr);
                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // Парсинг даты/времени
                    var timestamp = DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                        ? parsedDate
                        : DateTime.UtcNow;

                    // Создаем сообщение для обработки
                    var positionMessage = new PositionMessage
                    {
                        SsmpId = ssmpId,
                        CarNumber = carNumber,
                        DeviceId = deviceId,
                        Latitude = latitude,
                        Longitude = longitude,
                        Timestamp = timestamp
                    };

                    _logger.LogInformation(
                        "Обработка позиции: SSMP={SsmpId}, Машина={CarNumber}, Device={DeviceId}, Lat={Lat}, Lon={Lon}, Time={Time}",
                        ssmpId, carNumber, deviceId, latitude, longitude, timestamp);

                    // Обрабатываем сообщение через сервис (чистая архитектура)
                    using var scope = _serviceProvider.CreateScope();
                    var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();
                    await positionService.ProcessPositionAsync(positionMessage, stoppingToken);

                    _channel!.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения из RabbitMQ");
                    // В случае ошибки тоже делаем Ack, чтобы не зацикливаться на плохом сообщении
                    _channel?.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
