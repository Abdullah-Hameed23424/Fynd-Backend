using Fynd.Api.DTOs.Home;
using Fynd.Api.DTOs.Posts;
using Fynd.Api.Services;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetGetRecentPostsAsync()
        {
            var items = await _postService.GetRecentPostsAsync();

            return Ok(items);
        }
    }
}
