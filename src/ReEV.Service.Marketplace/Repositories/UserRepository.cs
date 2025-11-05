using Microsoft.EntityFrameworkCore;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;

namespace ReEV.Service.Marketplace.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<User> CreateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User?> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<User>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> UpdateAsync(Guid id, User entity)
        {
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.FullName = entity.FullName;
            existingUser.AvatarUrl = entity.AvatarUrl;
            existingUser.Balance = entity.Balance;
            existingUser.LockedBalance = entity.LockedBalance;
            existingUser.Status = entity.Status;

            await _appDbContext.SaveChangesAsync();
            return existingUser;
        }

        // Helper method để update user trực tiếp (không cần id vì entity đã có id)
        public async Task<User?> UpdateAsync(User entity)
        {
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.FullName = entity.FullName;
            if (entity.AvatarUrl != null)
            {
                existingUser.AvatarUrl = entity.AvatarUrl;
            }
            existingUser.Balance = entity.Balance;
            existingUser.LockedBalance = entity.LockedBalance;
            existingUser.Status = entity.Status;

            await _appDbContext.SaveChangesAsync();
            return existingUser;
        }
    }
}
