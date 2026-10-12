using Azure.Core;
using Fynd.Api.DTOs.Home;
using Fynd.Api.DTOs.Posts;
using Fynd.Api.Services;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetGetRecentPosts()
        {
            var items = await _postService.GetRecentPostsAsync();

            return Ok(items);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<PostResponse>>> GetGetPosts([FromQuery] PostQueryParameters parameters)
        {
            var result = await _postService.GetPostsAsync(parameters);

            return Ok(result);
        }

        [HttpGet("details")]
        public async Task<ActionResult<PostResponse>> GetPostDetails(
            [FromQuery] PostDetailsQueryParameters parameters)
        {
            var post = await _postService.GetPostDetailsAsync(parameters);

            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [HttpPost("create")]
        public async Task<ActionResult<PostResponse>> CreatePost(
            [FromBody] CreatePostRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _postService.CreatePostAsync(
                userId.Value,
                request);

            return CreatedAtAction(
                nameof(GetPostDetails),
                new
                {
                    id = item.Id,
                    type = item.Type
                },
                item);
        }

        [HttpGet("my-own")]
        public async Task<ActionResult<PagedPostResponse>> GetMyPosts(
            [FromQuery] PostQueryParameters parameters)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _postService.GetMyPostsAsync(
                userId.Value,
                parameters);

            return Ok(result);
        }

        [HttpPut("update-post/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id,[FromBody] UpdatePostRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var updated = await _postService.UpdatePostAsync(
                userId.Value,
                id,
                request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Post not found."
                });
            }

            return NoContent();
        }

        [HttpDelete("delete-post/{id}")]
        public async Task<IActionResult> Delete([FromQuery] PostType postType,[FromRoute] int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _postService.DeletePostAsync(postType, userId.Value, id);

            if (!deleted)
            {
                return NotFound(new { message = "Post not found." });
            }

            return Ok(new
            {
                message = "Post deleted successfully."
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return null;
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}
