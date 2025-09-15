using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using WeatherApiWrapper.Configuration;
using WeatherApiWrapper.Infrastructure.Resilience;
using WeatherApiWrapper.Interfaces;
using WeatherApiWrapper.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind settings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("WeatherApi"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ApiSettings>>().Value);

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetSection("Cache")["Redis"];
    opt.InstanceName = builder.Configuration.GetSection("Cache")["Prefix"];
});

// loggerFactory lazýmsa:
var lf = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var retry = Policies.GetRetryPolicy(lf.CreateLogger("PollyRetry"));
var breaker = Policies.GetCircuitBreakerPolicy(lf.CreateLogger("PollyCB"));
var timeout = Policies.GetTimeoutPolicy();

var pipeline = Polly.Policy.WrapAsync(breaker, retry, timeout);

builder.Services.AddHttpClient<WeatherService>()
    .AddPolicyHandler(pipeline)
    .SetHandlerLifetime(TimeSpan.FromMinutes(10));

builder.Services.AddHttpClient("polly-test")
    .AddPolicyHandler(pipeline)
    .SetHandlerLifetime(TimeSpan.FromMinutes(10));

builder.Services.AddTransient<IWeatherService>(sp =>
{
    var inner = sp.GetRequiredService<WeatherService>();
    var cache = sp.GetRequiredService<IDistributedCache>();
    var cs = sp.GetRequiredService<IOptions<CacheSettings>>();
    var logger = sp.GetRequiredService<ILogger<CachedWeatherService>>();
    return new CachedWeatherService(inner, cache, cs, logger);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
