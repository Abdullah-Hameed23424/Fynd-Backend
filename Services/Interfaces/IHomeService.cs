using Fynd.Api.DTOs.Home;

namespace Fynd.Api.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeResponse> GetHomeAsync();
    }
}
