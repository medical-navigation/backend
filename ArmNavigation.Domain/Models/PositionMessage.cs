namespace ArmNavigation.Domain.Models
{
    public class PositionMessage
    {
        public string SsmpId { get; set; } = string.Empty;
        public string CarNumber { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Для внутреннего использования после маппинга
        public Guid CarId { get; set; }
    }
}
