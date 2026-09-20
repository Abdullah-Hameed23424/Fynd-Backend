using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.DTOs.Home;
using Fynd.Api.Services;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            this._homeService = homeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HomeResponse>>> GetAll()
        {
            var items = await _homeService.GetHomeAsync();

            return Ok(items);
        }
    }
}
