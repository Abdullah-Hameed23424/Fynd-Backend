using System.Security.Claims;
using Fynd.Api.DTOs.LostItem;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LostItemsController : ControllerBase
    {
        private readonly ILostItemService _lostItemService;

        public LostItemsController(
            ILostItemService lostItemService)
        {
            _lostItemService = lostItemService;
        }

        [HttpPost]
        public async Task<ActionResult<LostItemResponse>> Create(
            CreateLostItemRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _lostItemService.CreateAsync(
                userId.Value,
                request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LostItemResponse>>> GetAll()
        {
            var items = await _lostItemService.GetAllAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<LostItemResponse>> GetById(
            int id)
        {
            var item = await _lostItemService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Lost item not found."
                });
            }

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateLostItemRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var updated = await _lostItemService.UpdateAsync(
                userId.Value,
                id,
                request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Lost item not found."
                });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _lostItemService.DeleteAsync(
                userId.Value,
                id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Lost item not found."
                });
            }

            return Ok(new
            {
                message = "Lost item deleted successfully."
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