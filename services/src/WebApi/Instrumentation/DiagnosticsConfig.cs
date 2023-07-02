using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WebApi.Instrumentation
{
    public static class DiagnosticsConfig
    {
        public const string ServiceName = "devops-workshop-webapi";
        public static Meter Meter { get; } = new (ServiceName);
        public static Histogram<double> ForecastTemperature { get; } = Meter.CreateHistogram<double>("forecast.temperature");
        public static Counter<long> FreezingDaysCounter { get; } = Meter.CreateCounter<long>("forecast.days.freezing");
        public static double LastForecastValue { get; set; }
        public static ObservableGauge<double> LastForecastValueGauge { get; } = Meter.CreateObservableGauge("forecast.last.value", () => LastForecastValue);
        public static ActivitySource ActivitySource { get; } = new (ServiceName);
    }
}
