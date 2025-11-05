using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            // Lấy user ID từ claims
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID claim not found"
                });
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID"
                });
            }

            var response = await _favouriteService.GetFavouritesAsync(userId, page, page_size);
            return Ok(response);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteFavourite(
            [FromQuery] string? listing_id = null,
            [FromQuery] string? favourite_id = null)
        {
            // Lấy user ID từ claims
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID claim not found"
                });
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID"
                });
            }

            // Kiểm tra ít nhất một trong hai param phải có
            if (string.IsNullOrEmpty(listing_id) && string.IsNullOrEmpty(favourite_id))
            {
                return BadRequest(new { 
                    message = "Either listing_id or favourite_id must be provided."
                });
            }

            Guid? listingIdGuid = null;
            Guid? favouriteIdGuid = null;

            // Parse listing_id nếu có
            if (!string.IsNullOrEmpty(listing_id))
            {
                if (!Guid.TryParse(listing_id, out Guid parsedListingId))
                {
                    return BadRequest(new { 
                        message = "Invalid listing_id format. Must be a valid GUID."
                    });
                }
                listingIdGuid = parsedListingId;
            }

            // Parse favourite_id nếu có
            if (!string.IsNullOrEmpty(favourite_id))
            {
                if (!Guid.TryParse(favourite_id, out Guid parsedFavouriteId))
                {
                    return BadRequest(new { 
                        message = "Invalid favourite_id format. Must be a valid GUID."
                    });
                }
                favouriteIdGuid = parsedFavouriteId;
            }

            try
            {
                await _favouriteService.DeleteFavouriteAsync(userId, listingIdGuid, favouriteIdGuid);
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
            // Lấy user ID từ claims
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID claim not found"
                });
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID"
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
                await _favouriteService.AddFavouriteAsync(userId, listingId);
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

