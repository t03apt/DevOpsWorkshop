using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
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
            var builder = WebApplication.CreateBuilder(args);
            var appConfigurationConnectionString = builder.Configuration["ApplicationConfiguration:ConnectionString"];
            var useAppConfigurationService = !string.IsNullOrEmpty(appConfigurationConnectionString);
            if (useAppConfigurationService)
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
            else
            {
                builder.Services.AddSingleton<IFeatureManager, InMemoryFeatureManager>();
            }

            builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program), ServiceLifetime.Singleton);
            builder.Services
                .AddOptions<ValidationDemoOptions>()
                .BindConfiguration(ValidationDemoOptions.SectionName)
                .ValidateFluently()
                .ValidateOnStart();

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

            var openTelemetryOptions = new OpenTelemetryOptions();
            builder.Configuration.GetSection(OpenTelemetryOptions.SectionName).Bind(openTelemetryOptions);
            if (openTelemetryOptions.Enabled)
            {
                var applicationInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
                var hasApplicationInsightsConnectionString = !string.IsNullOrEmpty(applicationInsightsConnectionString);
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
                        .ConfigureResource(appResourceBuilder);

                    if (openTelemetryOptions.UseConsoleExporter)
                    {
                        configure.AddConsoleExporter();
                    }

                    if (openTelemetryOptions.UseAzureMonitorExporters && hasApplicationInsightsConnectionString)
                    {
                        configure.AddAzureMonitorTraceExporter(o => o.ConnectionString = applicationInsightsConnectionString);
                    }

                    if (openTelemetryOptions.UseOtlpExporter)
                    {
                        configure.AddOtlpExporter();
                    }
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
                        .ConfigureResource(appResourceBuilder);

                    if (openTelemetryOptions.UseConsoleExporter)
                    {
                        configure.AddConsoleExporter();
                    }

                    if (openTelemetryOptions.UseAzureMonitorExporters && hasApplicationInsightsConnectionString)
                    {
                        configure.AddAzureMonitorMetricExporter(o => o.ConnectionString = applicationInsightsConnectionString);
                    }

                    if (openTelemetryOptions.UsePrometheusExporter)
                    {
                        configure.AddPrometheusExporter();
                    }

                    if (openTelemetryOptions.UseOtlpExporter)
                    {
                        configure.AddOtlpExporter();
                    }
                });

                builder.Services.AddLogging(l =>
                {
                    l.AddOpenTelemetry(configure =>
                    {
                        var resourceBuilder = ResourceBuilder.CreateDefault();
                        appResourceBuilder(resourceBuilder);
                        configure
                            .SetResourceBuilder(resourceBuilder);

                        if (openTelemetryOptions.UseConsoleExporter)
                        {
                            configure.AddConsoleExporter();
                        }

                        if (openTelemetryOptions.UseAzureMonitorExporters && hasApplicationInsightsConnectionString)
                        {
                            configure.AddAzureMonitorLogExporter(o => o.ConnectionString = applicationInsightsConnectionString);
                        }

                        if (openTelemetryOptions.UseOtlpExporter)
                        {
                            configure.AddOtlpExporter();
                        }
                    });
                });
            }
            else
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            var app = builder.Build();
            if (useAppConfigurationService)
            {
                app.UseAzureAppConfiguration();
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            if (openTelemetryOptions.Enabled && openTelemetryOptions.UsePrometheusExporter)
            {
                app.UseOpenTelemetryPrometheusScrapingEndpoint();
            }

            app.Run();
        }
    }
}
