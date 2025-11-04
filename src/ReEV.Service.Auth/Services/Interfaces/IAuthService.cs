using ReEV.Service.Auth.DTOs;

namespace ReEV.Service.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDTO> Register(UserCreateDTO registerDto);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginDto);
    }
}
