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

        public Task<User?> UpdateAsync(Guid id, User entity)
        {
            throw new NotImplementedException();
        }
    }
}
