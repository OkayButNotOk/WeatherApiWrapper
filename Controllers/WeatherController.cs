using Microsoft.AspNetCore.Mvc;
using WeatherApiWrapper.Interfaces;
using WeatherApiWrapper.Models;

namespace WeatherApiWrapper.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController(IWeatherService weatherService) : ControllerBase
    {
        private readonly IWeatherService _weatherService = weatherService;

        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var result = await _weatherService.GetWeatherAsync(city);
            if (result == null)
                return NotFound("Weather data not found");
            var dto = result.ToDto();

            return Ok(dto);
        }
    }
}

