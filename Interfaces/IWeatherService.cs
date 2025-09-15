using WeatherApiWrapper.Models;

namespace WeatherApiWrapper.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponse?> GetWeatherAsync(string city);
    }
}
