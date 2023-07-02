using FluentValidation;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApi.ColorManagment;
using WebApi.Demo;
using WebApi.Instrumentation;
using WebApi.Validation;
using WebApi.ValidationDemo;

namespace WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var isOpenTelemetryServicesEnabled = false;

            var builder = WebApplication.CreateBuilder(args);
            var appConfigurationConnectionString = builder.Configuration["ApplicationConfiguration:ConnectionString"];
            if (!string.IsNullOrEmpty(appConfigurationConnectionString))
            {
                builder.Services.AddAzureAppConfiguration();
                builder.Services.AddFeatureManagement()
                    .AddFeatureFilter<UserCountryFilter>();

                builder.Configuration.AddAzureAppConfiguration(appConfigOptions =>
                {
                    appConfigOptions.Connect(appConfigurationConnectionString);
                    appConfigOptions.Select(KeyFilter.Any, LabelFilter.Null);
                    appConfigOptions.Select(KeyFilter.Any, "dev");

                    appConfigOptions.ConfigureRefresh(appConfigRefresherOption =>
                    {
                        appConfigRefresherOption.SetCacheExpiration(TimeSpan.FromSeconds(5));
                        appConfigRefresherOption.Register("A:Refresh", true);
                    });

                    appConfigOptions.UseFeatureFlags(featureFlagOptions =>
                    {
                        featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(600);
                        featureFlagOptions.Select(KeyFilter.Any, LabelFilter.Null);
                    });

                    // appConfigOptions.ConfigureKeyVault(kv =>
                    // {

                    // var secretClient = new SecretClient(new Uri("https://kv-gf-pow.vault.azure.net/"), new DefaultAzureCredential());
                    //     kv.Register(secretClient);
                    // });
                });
            }

            builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program), ServiceLifetime.Singleton);
            builder.Services
                .AddOptions<ValidationDemoOptions>()
                .BindConfiguration(ValidationDemoOptions.SectionName)
                .ValidateFluently()
                .ValidateOnStart();

            builder.Services.AddApplicationInsightsTelemetry();

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();

            builder.Services.AddTransient<IColorService, ColorService>();

            var azureStorageConnectionString = builder.Configuration.GetConnectionString("AzureStorage");
            if (!string.IsNullOrEmpty(azureStorageConnectionString))
            {
                builder.Services.AddHostedService<QueueListenerBackgroundService>();
            }

            Action<ResourceBuilder> appResourceBuilder = resource => resource.AddService(DiagnosticsConfig.ServiceName);

            if (isOpenTelemetryServicesEnabled)
            {
                builder.Services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder =>
                    resourceBuilder
                        .AddService(DiagnosticsConfig.ServiceName)
                        .AddAttributes(new List<KeyValuePair<string, object>>
                        {
                            new ("my-attribute", "my-value")
                        }))
                .WithTracing(configure =>
                {
                    configure
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource(DiagnosticsConfig.ActivitySource.Name)
                        .ConfigureResource(appResourceBuilder)

                        // .AddConsoleExporter()
                        .AddOtlpExporter(ConfigureOtlpExporter);

                    configure.AddOtlpExporter();
                })
                .WithMetrics(configure =>
                {
                    configure.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
                    configure.AddView(
                        "request-duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                        });

                    configure
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()

                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()

                        .AddMeter(DiagnosticsConfig.Meter.Name)
                        .ConfigureResource(appResourceBuilder)
                        .AddOtlpExporter(ConfigureOtlpExporter)
                        .AddPrometheusExporter();
                });

                builder.Services.AddLogging(l =>
                {
                    l.AddOpenTelemetry(configure =>
                    {
                        var resourceBuilder = ResourceBuilder.CreateDefault();
                        appResourceBuilder(resourceBuilder);
                        configure
                            .SetResourceBuilder(resourceBuilder)

                            // .AddConsoleExporter()
                            .AddOtlpExporter(ConfigureOtlpExporter);
                    });
                });
            }
            else
            {
                builder.Logging.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) =>
                        config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"],
                    configureApplicationInsightsLoggerOptions: (options) => { });

                // builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("your-category", LogLevel.Trace);
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            if (isOpenTelemetryServicesEnabled)
            {
                app.UseOpenTelemetryPrometheusScrapingEndpoint();
            }

            app.UseAzureAppConfiguration();

            app.Run();
        }

        private static void ConfigureOtlpExporter(OtlpExporterOptions o)
        {
            // o.ExportProcessorType = ExportProcessorType.Simple;
            o.Endpoint = new Uri("http://localhost:4317");
        }
    }
}
