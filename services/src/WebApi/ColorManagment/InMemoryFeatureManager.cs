using Microsoft.FeatureManagement;

namespace WebApi.ColorManagment
{
    public class InMemoryFeatureManager : IFeatureManager
    {
        public async IAsyncEnumerable<string> GetFeatureNamesAsync()
        {
            await Task.CompletedTask;
            yield break;
        }

        public Task<bool> IsEnabledAsync(string feature)
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
        {
            return Task.FromResult(false);
        }
    }
}
