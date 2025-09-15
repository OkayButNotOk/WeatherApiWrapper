namespace WeatherApiWrapper.Models
{
    public static class Mapper
    {
        public static WeatherDto ToDto(this WeatherResponse r)
        {
            // timezone: saniye -> offset
            var offset = (r.Timezone ?? 0);
            var tz = TimeSpan.FromSeconds(offset);

            DateTimeOffset? FromUnix(long? s) => s.HasValue ? DateTimeOffset.FromUnixTimeSeconds(s.Value).ToOffset(tz) : null;

            return new WeatherDto
            {
                City = r.Name,
                Country = r.Sys?.Country,
                Temp = r.Main?.Temp,
                FeelsLike = r.Main?.FeelsLike,
                Description = r.Weather?.FirstOrDefault()?.Description,
                WindSpeed = r.Wind?.Speed,
                TimeLocal = FromUnix(r.Dt),
                SunriseLocal = FromUnix(r.Sys?.Sunrise),
                SunsetLocal = FromUnix(r.Sys?.Sunset)
            };
        }
    }
}
