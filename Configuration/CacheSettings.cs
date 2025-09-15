namespace WeatherApiWrapper.Configuration
{
    public class CacheSettings
    {
        public string Redis { get; set; } = string.Empty; // Redis connection string
        public int Minutes { get; set; } = 60; // Default expiration time of 60 minutes
        public string Prefix { get; set; } = string.Empty; // Default cache key prefix
    }
}
