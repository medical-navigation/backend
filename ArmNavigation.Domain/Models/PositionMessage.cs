namespace ArmNavigation.Domain.Models
{
    public class PositionMessage
    {
        public Guid CarId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
