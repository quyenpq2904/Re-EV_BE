using ReEV.Service.Auth.DTOs;

namespace ReEV.Service.Auth.Services.Interfaces
{
    public interface IUserService
    {
        Task<PaginationResult<UserDTO>> GetUsers(int page = 1, int pageSize = 10, string search = "");
        Task<UserDTO?> GetUserById(Guid id);
        Task<UserDTO?> UpdateUser(Guid id, UserUpdateDTO userUpdateDto);
        Task<UserDTO> CreateUser(UserCreateDTO userCreateDto);

    }
}
