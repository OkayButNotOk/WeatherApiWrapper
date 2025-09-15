namespace WeatherApiWrapper.Models
{
    public class WeatherDto
    {
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Temp { get; set; }
        public double? FeelsLike { get; set; }
        public string? Description { get; set; }
        public double? WindSpeed { get; set; }
        public DateTimeOffset? TimeLocal { get; set; }
        public DateTimeOffset? SunriseLocal { get; set; }
        public DateTimeOffset? SunsetLocal { get; set; }
    }
}
