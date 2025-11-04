using Microsoft.AspNetCore.Mvc;
using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Services.Interfaces;

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
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var response = await _service.GetUsers(page, pageSize, search);
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
