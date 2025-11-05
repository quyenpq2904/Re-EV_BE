using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReEV.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 10,
            [FromQuery] string search = "")
        {
            var response = await _service.GetUsers(page, page_size, search);
            return Ok(response);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await _service.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(user);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserById([FromRoute] Guid id, [FromBody] UserUpdateDTO userUpdateDto)
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

            if (!Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID"
                });
            }

            // Lấy role từ claims
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value 
                ?? User.FindFirst("role")?.Value;

            // Kiểm tra: chỉ owner hoặc admin mới được update
            if (currentUserId != id && (string.IsNullOrEmpty(roleClaim) || roleClaim != "ADMIN"))
            {
                return Forbid("Only the account owner or ADMIN can update this user.");
            }

            var updatedUser = await _service.UpdateUser(id, userUpdateDto);
            if (updatedUser == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(updatedUser);
        }
    }
}
