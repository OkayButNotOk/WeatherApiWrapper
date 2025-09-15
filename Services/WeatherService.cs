using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WeatherApiWrapper.Configuration;
using WeatherApiWrapper.Interfaces;
using WeatherApiWrapper.Models;

namespace WeatherApiWrapper.Services
{
    public class WeatherService(HttpClient httpClient, ApiSettings apiSettings) : IWeatherService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiSettings _apiSettings = apiSettings;

        public async Task<WeatherResponse?> GetWeatherAsync(string city)
        {
            var requestUrl = $"{_apiSettings.BaseUrl}?q={city}&appid={_apiSettings.ApiKey}&units=metric&lang=tr";
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherResponse>(content);
        }
    }
}
