using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Models;
using System.Security.Claims;

namespace ReEV.Service.Auth.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDTO> GenerateTokensAsync(User user);
        Task<TokenResponseDTO> RenewToken(string accessToken, string refreshToken);
    }
}
