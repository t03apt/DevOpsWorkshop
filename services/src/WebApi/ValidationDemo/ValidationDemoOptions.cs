namespace WebApi.ValidationDemo
{
    public sealed class ValidationDemoOptions
    {
        public const string SectionName = "ValidationDemoOptions";
        public long? UnixTimeSeconds { get; set; }
        public DateTimeOffset? IsoDate { get; set; }
    }
}
