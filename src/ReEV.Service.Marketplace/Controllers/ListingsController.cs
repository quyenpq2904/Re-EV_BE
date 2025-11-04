using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace ReEV.Service.Marketplace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;
        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateListing([FromBody] CreateListingDTO dto)
        {
            var sellerIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (sellerIdClaim is null || !Guid.TryParse(sellerIdClaim, out var sellerId))
            {
                return Unauthorized();
            }
            ;

            var listing = await _listingService.CreateListingAsync(sellerId, dto);
            return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing);
        }

        [HttpGet("")]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetListingById([FromRoute] Guid id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing is null)
            {
                return NotFound();
            }
            return Ok(listing);
        }
    }
}
