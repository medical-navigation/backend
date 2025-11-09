namespace ArmNavigation.Domain.Models
{
    public sealed record Point
    {
        public Point(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
