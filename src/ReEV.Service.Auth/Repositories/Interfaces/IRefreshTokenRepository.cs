using ReEV.Service.Auth.Models;

namespace ReEV.Service.Auth.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetRefreshTokenByUserIdAndToken(Guid userId, string token);
        Task<RefreshToken?> UpdateAsync(Guid id);
    }
}
