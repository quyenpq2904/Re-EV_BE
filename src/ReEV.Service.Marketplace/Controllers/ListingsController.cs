using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;

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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateListing([FromForm] CreateListingDTO dto)
        {
            var sellerIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(sellerIdClaim) || !Guid.TryParse(sellerIdClaim, out var sellerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            try
            {
                var listing = await _listingService.CreateListingAsync(sellerId, dto);
                return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllListings(
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 10,
            [FromQuery] string search = "")
        {
            var response = await _listingService.GetAllListingsAsync(page, page_size, search);
            return Ok(response);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetListingById([FromRoute] Guid id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing is null)
            {
                return NotFound(new { message = "Listing not found" });
            }
            return Ok(listing);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateListing(
            [FromRoute] Guid id,
            [FromForm] UpdateListingDTO dto)
        {
            var sellerIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(sellerIdClaim) || !Guid.TryParse(sellerIdClaim, out var sellerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            try
            {
                var updatedListing = await _listingService.UpdateListingAsync(id, sellerId, dto);
                if (updatedListing == null)
                {
                    return NotFound(new { message = "Listing not found" });
                }
                return Ok(updatedListing);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        [Route("{id:Guid}/verify")]
        [Authorize]
        public async Task<IActionResult> VerifyListing([FromRoute] Guid id)
        {
            // Kiểm tra role ADMIN từ token claims
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value 
                ?? User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(roleClaim) || roleClaim != "ADMIN")
            {
                return Forbid("Only ADMIN users can verify listings.");
            }

            try
            {
                var listing = await _listingService.VerifyListingAsync(id, true);
                if (listing == null)
                {
                    return NotFound(new { message = "Listing not found" });
                }
                return Ok(listing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("{id:Guid}/verify")]
        [Authorize]
        public async Task<IActionResult> UnverifyListing([FromRoute] Guid id)
        {
            // Kiểm tra role ADMIN từ token claims
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value 
                ?? User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(roleClaim) || roleClaim != "ADMIN")
            {
                return Forbid("Only ADMIN users can unverify listings.");
            }

            try
            {
                var listing = await _listingService.VerifyListingAsync(id, false);
                if (listing == null)
                {
                    return NotFound(new { message = "Listing not found" });
                }
                return Ok(listing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
