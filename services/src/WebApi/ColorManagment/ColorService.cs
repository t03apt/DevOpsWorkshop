using Microsoft.FeatureManagement;

namespace WebApi.ColorManagment
{
    public class ColorService : IColorService
    {
        private readonly IConfiguration _configuration;
        private readonly IFeatureManager _featureManager;

        public ColorService(
            IConfiguration configuration,
            IFeatureManager featureManager)
        {
            _configuration = configuration;
            _featureManager = featureManager;
        }

        public async Task<Color> GetColorAsync()
        {
            var niceColor = GetSystemColor();

            if (await _featureManager.IsEnabledAsync("randomColor"))
            {
                var random = new Random();
                return new Color()
                {
                    R = (byte)random.Next(255),
                    G = (byte)random.Next(255),
                    B = (byte)random.Next(255),
                    A = (byte)random.Next(255),
                };
            }

            return new Color()
            {
                R = niceColor.R,
                G = niceColor.G,
                B = niceColor.B,
                A = niceColor.A,
            };
        }

        private System.Drawing.Color GetSystemColor() =>
            _configuration["color"] == "green" ? System.Drawing.Color.Green : System.Drawing.Color.White;
    }
}
