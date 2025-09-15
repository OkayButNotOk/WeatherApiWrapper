using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;

namespace WeatherApiWrapper.Infrastructure.Resilience
{
    public static class Policies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
        {
            var delay = Backoff.ExponentialBackoff(TimeSpan.FromMilliseconds(200), retryCount: 3);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // Polly.Timeout
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(delay, (result, timeSpan, retryCount, context) =>
                {
                    var code = result.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError;
                    logger.LogWarning("HTTP retry {retry} in {delay} (status:{status})", retryCount, timeSpan, code);
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // Polly.Timeout
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,             // 5 ardışık hata devre kes
                    durationOfBreak: TimeSpan.FromSeconds(30),         // 30 sn kitli kalsın
                    onBreak: (outcome, breakDelay) =>
                        logger.LogError("Circuit OPEN for {delay}. Status:{status}", breakDelay, outcome.Result?.StatusCode),
                    onReset: () => logger.LogInformation("Circuit RESET"),
                    onHalfOpen: () => logger.LogInformation("Circuit HALF-OPEN"));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));
    }
}

