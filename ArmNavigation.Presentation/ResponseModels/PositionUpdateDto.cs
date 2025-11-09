namespace ArmNavigation.Presentation.ResponseModels
{
    /// <summary>
    /// Данные о позиции машины, отправляемые через SignalR Hub
    /// </summary>
    /// <remarks>
    /// Эти данные отправляются в real-time через SignalR на эндпоинт /hubs/position
    /// при получении обновлений от RabbitMQ.
    ///
    /// Пример подключения из JavaScript:
    /// <code>
    /// const connection = new signalR.HubConnectionBuilder()
    ///     .withUrl("http://localhost:8080/hubs/position")
    ///     .build();
    ///
    /// connection.on("ReceivePosition", (position) => {
    ///     console.log("Новая позиция:", position);
    /// });
    ///
    /// await connection.start();
    /// </code>
    /// </remarks>
    public class PositionUpdateDto
    {
        /// <summary>
        /// ID станции скорой медицинской помощи
        /// </summary>
        /// <example>12</example>
        public string SsmpId { get; set; } = string.Empty;

        /// <summary>
        /// Номер машины (регистрационный номер)
        /// </summary>
        /// <example>м280ох159</example>
        public string CarNumber { get; set; } = string.Empty;

        /// <summary>
        /// UUID машины в базе данных
        /// </summary>
        /// <example>b3d61d21-1567-4c14-89b1-bafcc5655cce</example>
        public Guid? CarId { get; set; }

        /// <summary>
        /// ID GPS-устройства
        /// </summary>
        /// <example>869270042673490</example>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Широта (Latitude)
        /// </summary>
        /// <example>57.569873</example>
        public double Latitude { get; set; }

        /// <summary>
        /// Долгота (Longitude)
        /// </summary>
        /// <example>57.202026</example>
        public double Longitude { get; set; }

        /// <summary>
        /// Время события (когда была зафиксирована позиция)
        /// </summary>
        /// <example>2025-11-05T11:46:37</example>
        public DateTime Timestamp { get; set; }
    }
}
