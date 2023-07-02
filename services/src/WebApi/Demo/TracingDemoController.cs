using Microsoft.AspNetCore.Mvc;
using WebApi.Instrumentation;

namespace WebApi.Demo
{
    [ApiController]
    [Route("[controller]")]
    public class TracingDemoController : ControllerBase
    {
        private readonly ILogger _logger;

        public TracingDemoController(ILogger<TracingDemoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route(nameof(TracingExample1))]
        public async Task<ActionResult> TracingExample1()
        {
            using var requestActivity = DiagnosticsConfig.ActivitySource.StartActivity(nameof(TracingExample1));
            await Task.Delay(500);

            using (var dependencyActivity = DiagnosticsConfig.ActivitySource.StartActivity("Some dependency"))
            {
                await Task.Delay(500);
            }

            await Task.Delay(500);
            return Ok();
        }

        [HttpGet]
        [Route(nameof(TracingExample2))]
        public async Task<ActionResult> TracingExample2()
        {
            using var requestActivity = DiagnosticsConfig.ActivitySource.StartActivity(nameof(TracingExample2));
            var random = new Random();
            var delays = Enumerable.Range(1, 50).Select(_ => TimeSpan.FromSeconds((1 + random.NextDouble()) * 4)).ToArray();
            var tasks = new List<Task>();
            for (var i = 0; i < delays.Length; i++)
            {
                var index = i;
                var delay = delays[index];
                tasks.Add(Task.Run(async () =>
                {
                    using var dependencyActivity = DiagnosticsConfig.ActivitySource.StartActivity($"#{index} dependency");
                    dependencyActivity?.SetTag("delay", delay.ToString());

                    _logger.LogDebug("(logger) Waiting {Delay}", delay);
                    await Task.Delay(delay);
                }));
            }

            await Task.WhenAll(tasks);
            return Ok();
        }
    }
}
