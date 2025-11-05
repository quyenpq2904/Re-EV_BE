using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Helpers;
using ReEV.Service.Marketplace.Services.Interfaces;

namespace ReEV.Service.Marketplace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidsController : ControllerBase
    {
        private readonly IBidService _bidService;

        public BidsController(IBidService bidService)
        {
            _bidService = bidService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBid([FromBody] CreateBidDTO dto)
        {
            // Lấy user ID từ claims (được populate từ Gateway headers X-User-Id)
            var bidderId = UserHelper.GetUserId(User);
            if (bidderId == null)
            {
                return Unauthorized(new { message = "User ID not found in request headers" });
            }

            try
            {
                var bid = await _bidService.CreateBidAsync(bidderId.Value, dto);
                return CreatedAtAction(nameof(GetBidById), new { id = bid.Id }, bid);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the bid.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBids(
            [FromQuery] string? listing_id = null,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 10)
        {
            Guid? listingIdGuid = null;
            if (!string.IsNullOrEmpty(listing_id))
            {
                if (!Guid.TryParse(listing_id, out Guid parsedListingId))
                {
                    return BadRequest(new { message = "Invalid listing_id format. Must be a valid GUID." });
                }
                listingIdGuid = parsedListingId;
            }

            try
            {
                var response = await _bidService.GetBidsAsync(listingIdGuid, page, page_size);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving bids.", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetBidById([FromRoute] Guid id)
        {
            // TODO: Implement if needed
            return NotFound(new { message = "Not implemented yet" });
        }
    }
}

