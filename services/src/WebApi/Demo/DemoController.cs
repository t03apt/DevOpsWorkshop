using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using SampleNuget;
using WebApi.ColorManagment;
using WebApi.Instrumentation;

namespace WebApi.Demo
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IColorService _colorService;

        public DemoController(ILogger<DemoController> logger, IHttpClientFactory httpClientFactory, IColorService colorService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _colorService = colorService;
        }

        [HttpGet("samplenuget")]
        public IEnumerable<int> SampleNuget()
        {
            var numbers = new List<int>();
            MyAwesomeService.Fibonacci(10, (number) =>
            {
                numbers.Add(number);
            });

            return numbers;
        }

        [HttpPost("gccollect")]
        public void GcCollect()
        {
            GC.Collect();
        }

        [HttpGet("writelogs")]
        public ActionResult WriteLogs()
        {
            _logger.LogDebug($"!Method {nameof(WriteLogs)} has been called.");
            _logger.LogDebug("Method {MethodName} has been called.", nameof(WriteLogs));

            _logger.LogDebug($"!CPU level: {80}, MemoryUsage: {200}");
            _logger.LogDebug("CPU level: {CPULevel}, MemoryUsage: {MemoryUsage}", 80, 200);

            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["OperationName"] = "Complex Operation",
                    ["CorrelationId"] = Guid.NewGuid()
                }))
            {
                _logger.LogDebug("Operation started.");
                _logger.LogDebug("Operation completed.");
            }

            _logger.LogError(new InvalidOperationException("Something is in a bad state"), "Something went wrong!");

            return Ok();
        }

        // GET api/<Class>/5
        [HttpGet("weatherforecast")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Ignore")]
        public IEnumerable<WeatherForecast> GetWeatherForecast()
        {
            using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Creating forecast");
            activity?.SetTag("custom-tag", "custom-value");

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var res = httpClient.GetStringAsync("http://google.com").Result;

                var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                })
                .ToArray();

                activity?.AddEvent(
                    new ActivityEvent(
                        "Forecast created",
                        tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
                        {
                            new ("forecast.count", forecast.Length)
                        })));

                DiagnosticsConfig.FreezingDaysCounter.Add(forecast.Count(f => f.TemperatureC < 0));
                foreach (var item in forecast)
                {
                    DiagnosticsConfig.ForecastTemperature.Record(item.TemperatureC);
                }

                DiagnosticsConfig.LastForecastValue = forecast.Last().TemperatureC;

                return forecast;
            }
            catch (Exception ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.RecordException(ex, new TagList
                {
                    new ("custom-error-tag", "custom-value")
                });

                throw;
            }
        }
    }
}
