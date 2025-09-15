using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WeatherApiWrapper.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TestController> _logger;

        public TestController(IHttpClientFactory httpClientFactory, ILogger<TestController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("sleep")]
        // A method to simulate a delay to use it in timeout testing, circuit breaker, etc.
        public async Task<IActionResult> Sleep([FromQuery] int ms = 10_000)
        {
            await Task.Delay(ms);
            return Ok(new { slept = ms });
        }

        [HttpGet("timeout")]
        public async Task<IActionResult> GetWithTimeout([FromQuery] int seconds = 10)
        {
            var client = _httpClientFactory.CreateClient("polly-test");
            var baseHttp = "https://localhost:7125";
            var url = $"{baseHttp}/api/test/sleep?ms={seconds * 1000}";

            _logger.LogInformation("TEST timeout url={url}", url);

            try
            {
                var resp = await client.GetAsync(url);
                var content = await resp.Content.ReadAsStringAsync();

                return new ObjectResult(new { status = resp.StatusCode, content })
                {
                    StatusCode = (int)resp.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Timeout test failed");
                return StatusCode((int)HttpStatusCode.GatewayTimeout,
                    new { error = ex.GetType().Name, message = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status([FromQuery] int code = 500)
        {
            var client = _httpClientFactory.CreateClient("polly-test");
            var url = $"http://httpstat.us/{code}";

            _logger.LogInformation("TEST status url={url}", url);

            var resp = await client.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();

            return new ObjectResult(new { status = resp.StatusCode, content })
            {
                StatusCode = (int)resp.StatusCode
            };
        }

        [HttpGet("unreachable")]
        public async Task<IActionResult> Unreachable()
        {
            var client = _httpClientFactory.CreateClient("polly-test");
            var url = "http://10.255.255.1/";
            _logger.LogInformation("TEST unreachable url={url}", url);

            try
            {
                var resp = await client.GetAsync(url);
                var content = await resp.Content.ReadAsStringAsync();

                return new ObjectResult(new { status = resp.StatusCode, content })
                {
                    StatusCode = (int)resp.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unreachable test failed");
                return StatusCode((int)HttpStatusCode.BadGateway,
                    new { error = ex.GetType().Name, message = ex.Message });
            }
        }
    }
    
}
