using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WeatherApiWrapper.Configuration;
using WeatherApiWrapper.Interfaces;
using WeatherApiWrapper.Models;

namespace WeatherApiWrapper.Services
{
    public class CachedWeatherService(WeatherService innerService, IDistributedCache cache, IOptions<CacheSettings> cacheSettings, ILogger<CachedWeatherService> logger) : IWeatherService
    {
        private readonly WeatherService _innerService = innerService;
        private readonly IDistributedCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;
        private readonly ILogger<CachedWeatherService> _logger = logger;
        private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public async Task<WeatherResponse?> GetWeatherAsync(string city)
        {
            var key = $"{_cacheSettings.Prefix}{city.Trim().ToLowerInvariant()}";

            var cachedData = await _cache.GetAsync(key);
            if (cachedData != null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", key);
                var cachedJson = Encoding.UTF8.GetString(cachedData);
                return JsonSerializer.Deserialize<WeatherResponse>(cachedJson, _json);
            }

            _logger.LogInformation("CACHE MISS: {Key}", key);
            var fresh = await _innerService.GetWeatherAsync(city);
            if (fresh is null) return null;

            var json = JsonSerializer.Serialize(fresh, _json);
            await _cache.SetAsync(
                key,
                Encoding.UTF8.GetBytes(json),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.Minutes)
                });

            _logger.LogInformation("CACHE SET: {Key} (TTL: {Minutes}m)", key, _cacheSettings.Minutes);
            return fresh;
        }
    }
}
