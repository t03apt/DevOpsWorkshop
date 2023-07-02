namespace WebApi.Instrumentation
{
    public class OpenTelemetryOptions
    {
        public const string SectionName = "OpenTelemetry";
        public bool Enabled { get; set; }
        public bool UseConsoleExporter { get; set; }
        public bool UseAzureMonitorExporters { get; set; }
        public bool UseOtlpExporter { get; set; }
        public bool UsePrometheusExporter { get; set; }
    }
}
