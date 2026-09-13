using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FoundItemsController : ControllerBase
    {
        private readonly IFoundItemService _foundItemService;

        public FoundItemsController(IFoundItemService foundItemService)
        {
            this._foundItemService = foundItemService;
        }

        [HttpPost]
        public async Task<ActionResult<FoundItemResponse>> Create(CreateFoundItemRequest request)
        {
            var userId = GetCurrentUserId();

            if(userId == null)
            {
                return Unauthorized();
            }

            var item = await _foundItemService.CreateAsync(
                userId.Value,
                request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FoundItemResponse>>> GetAll()
        {
            var items  = await _foundItemService.GetAllAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoundItemResponse>> GetById(int id)
        {
            var item = await _foundItemService.GetByIdAsync(id);

            if(item == null)
            {
                return NotFound(
                    new
                    {
                        message = "Found item not found."
                    });
            }

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateFoundItemRequest request)
        {
            var userId = GetCurrentUserId();

            if(userId == null)
            {
                return Unauthorized();
            }

            var updated = await _foundItemService.UpdateAsync(
                userId.Value,
                id,
                request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Found item not found."
                });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _foundItemService.DeleteAsync(userId.Value, id);

            if (!deleted)
            {
                return NotFound(new { message = "Found item not found." });
            }

            return Ok(new
            {
                message = "Found item deleted successfully."
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier);

            if(userIdClaim == null)
            {
                return null;
            }

            if(!int.TryParse(userIdClaim.Value,out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}