using Microsoft.EntityFrameworkCore;
using ReEV.Service.Auth.Models;
using ReEV.Service.Auth.Repositories.Interfaces;

namespace ReEV.Service.Auth.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _appDbContext;
        public RefreshTokenRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken entity)
        {
            await _appDbContext.RefreshTokens.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public Task<RefreshToken?> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<RefreshToken>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            throw new NotImplementedException();
        }

        public Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<RefreshToken?> UpdateAsync(Guid id, RefreshToken entity)
        {
            throw new NotImplementedException();
        }

        public async Task<RefreshToken?> GetRefreshTokenByUserIdAndToken(Guid userId, string token)
        {
            return await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token);

        }

        public async Task<RefreshToken?> UpdateAsync(Guid id)
        {
            var existingToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);

            if (existingToken is null)
            {
                return null;
            }

            existingToken.IsActive = false;

            await _appDbContext.SaveChangesAsync();
            return existingToken;
        }
    }
}
