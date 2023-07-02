using Microsoft.Extensions.Configuration;

namespace AzureStorageSample.Tests
{
    [TestCaseOrderer("AzureStorageSamples.AlphabeticalOrdererTestCaseOrderer", "AzureStorageSamples")]
    public abstract class TestBase
    {
        protected TestBase()
        {
            Configuration = GetConfiguration();
            AzureStorageConnectionString = Configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("AzureStorage connection string not found");
        }

        public IConfiguration Configuration { get; set; }
        public string AzureStorageConnectionString { get; set; }

        public string Randomize(string prefix = "sample") => $"{prefix}-{Guid.NewGuid()}";

        private static IConfiguration GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddUserSecrets(typeof(Utilities).Assembly);
            var configuration = configurationBuilder.Build();

            return configuration;
        }
    }
}
