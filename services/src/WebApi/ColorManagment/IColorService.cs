namespace WebApi.ColorManagment
{
    public interface IColorService
    {
        Task<Color> GetColorAsync();
    }
}
