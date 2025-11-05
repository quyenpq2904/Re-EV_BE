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
        [Route("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            // Thử nhiều cách lấy user ID từ claims
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Log tất cả claims để debug
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                return Unauthorized(new { 
                    message = "Invalid token: User ID claim not found",
                    claims = allClaims,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    authenticationType = User.Identity?.AuthenticationType
                });
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID",
                    userIdClaim = userIdClaim
                });
            }

            var user = await _service.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        [HttpPut]
        [Route("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UserUpdateDTO userUpdateDto)
        {
            // Thử nhiều cách lấy user ID từ claims (giống như GetMe)
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Log tất cả claims để debug
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                return Unauthorized(new { 
                    message = "Invalid token: User ID claim not found",
                    claims = allClaims,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    authenticationType = User.Identity?.AuthenticationType
                });
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { 
                    message = "Invalid token: User ID is not a valid GUID",
                    userIdClaim = userIdClaim
                });
            }

            var updatedUser = await _service.UpdateUser(userId, userUpdateDto);
            if (updatedUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(updatedUser);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] Guid? user_id = null,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 10,
            [FromQuery] string search = "")
        {
            // Nếu có user_id thì trả về user theo ID
            if (user_id.HasValue)
            {
                var user = await _service.GetUserById(user_id.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }

            // Nếu không có user_id thì phân trang + search
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
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> UpdateUserById([FromRoute] Guid id, [FromBody] UserUpdateDTO userUpdateDto)
        {
            var updatedUser = await _service.UpdateUser(id, userUpdateDto);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(updatedUser);
        }
    }
}
