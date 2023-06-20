namespace WebApi.ColorManagment
{
    public class ColorService : IColorService
    {
        private readonly IConfiguration _configuration;

        public ColorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Color GetColor()
        {
            var niceColor = GetSystemColor();

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
