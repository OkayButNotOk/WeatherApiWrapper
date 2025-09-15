# WeatherApiWrapper

A resilient and cached .NET 8 Web API wrapper around the OpenWeather API.  
This project demonstrates API integration, caching with Redis, and resilience patterns using Polly.

## Features
- Fetch current weather data from OpenWeather API
- Redis caching with configurable expiration
- Resilience patterns:
  - Retry policy with exponential backoff
  - Circuit breaker policy
  - Timeout policy
- Structured logging
- Swagger documentation

## Requirements
- .NET 8 SDK
- Docker (for Redis)
- OpenWeather API key

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/OkayButNotOk/WeatherApiWrapper.git
   cd WeatherApiWrapper
   ```

2. Configure Redis and API key in `appsettings.json` or secrets:
   ```json
   {
     "WeatherApi": {
       "BaseUrl": "https://api.openweathermap.org/data/2.5/weather",
       "ApiKey": "YOUR_API_KEY"
     },
     "Cache": {
       "Redis": "localhost:6379",
       "Prefix": "weather:",
       "Minutes": 5
     }
   }
   ```

3. Run Redis with Docker:
   ```bash
   docker run -d -p 6379:6379 redis
   ```

4. Run the project:
   ```bash
   dotnet run
   ```

5. Open Swagger UI at:
   ```
   https://localhost:7125/swagger
   ```

## Endpoints

### `/api/weather/{city}`
Fetch weather for a given city (with caching and resilience).

### `/api/test/timeout?seconds=10`
Simulate a timeout scenario.

### `/api/test/status?code=500`
Simulate error status codes.

### `/api/test/unreachable`
Simulate unreachable host.

## License
This project is licensed under the MIT License.
