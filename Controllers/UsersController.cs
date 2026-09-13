using Fynd.Api.DTOs.User;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userService.GetByIdAsync(userId.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                createdAt = user.CreatedAt
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(
            UpdateUserRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var updated = await _userService.UpdateProfileAsync(
                 userId.Value,
                 request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _userService
                .DeleteMyAccountAsync(userId.Value);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "Account deleted successfully."
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

            if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}