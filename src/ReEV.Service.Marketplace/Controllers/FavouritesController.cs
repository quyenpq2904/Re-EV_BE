using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Helpers;
using ReEV.Service.Marketplace.Services.Interfaces;

namespace ReEV.Service.Marketplace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;

        public FavouritesController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFavourites(
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 10)
        {
            // Lấy user ID từ claims (được populate từ Gateway headers X-User-Id)
            var userId = UserHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(new { 
                    message = "User ID not found in request headers"
                });
            }

            var response = await _favouriteService.GetFavouritesAsync(userId.Value, page, page_size);
            return Ok(response);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteFavourite([FromQuery] string listing_id)
        {
            // Lấy user ID từ claims (được populate từ Gateway headers X-User-Id)
            var userId = UserHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(new { 
                    message = "User ID not found in request headers"
                });
            }

            // Kiểm tra listing_id có được cung cấp không
            if (string.IsNullOrEmpty(listing_id))
            {
                return BadRequest(new { 
                    message = "listing_id is required."
                });
            }

            // Parse listing_id
            if (!Guid.TryParse(listing_id, out Guid listingIdGuid))
            {
                return BadRequest(new { 
                    message = "Invalid listing_id format. Must be a valid GUID."
                });
            }

            try
            {
                await _favouriteService.DeleteFavouriteAsync(userId.Value, listingIdGuid);
                return Ok(new { 
                    message = "Favourite deleted successfully" 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddFavourite([FromBody] AddFavouriteRequestDTO dto)
        {
            // Lấy user ID từ claims (được populate từ Gateway headers X-User-Id)
            var userId = UserHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(new { 
                    message = "User ID not found in request headers"
                });
            }

            // Parse listing_id từ string sang Guid
            if (!Guid.TryParse(dto.listing_id, out Guid listingId))
            {
                return BadRequest(new { 
                    message = "Invalid listing_id format. Must be a valid GUID."
                });
            }

            try
            {
                await _favouriteService.AddFavouriteAsync(userId.Value, listingId);
                return Ok(new { 
                    message = "Listing added to favourites successfully" 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

