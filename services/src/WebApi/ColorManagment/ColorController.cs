using Microsoft.AspNetCore.Mvc;

namespace WebApi.ColorManagment
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _colorService;

        public ColorController(IColorService colorService)
        {
            _colorService = colorService;
        }

        [HttpGet("backgroundcolor")]
        public Color GetBackgroundColor()
        {
            return _colorService.GetColor();
        }
    }
}
