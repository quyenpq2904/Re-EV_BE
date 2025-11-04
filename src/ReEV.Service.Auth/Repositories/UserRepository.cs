using Microsoft.EntityFrameworkCore;
using ReEV.Service.Auth.Models;
using ReEV.Service.Auth.Repositories.Interfaces;

namespace ReEV.Service.Auth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<PaginationResult<User>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var users = _appDbContext.Users.AsQueryable();

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                users = users.Where(x => x.FullName.Contains(search));
            }

            var totalCount = users.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;

            var items = await users.Skip(skip).Take(pageSize).ToListAsync();

            return new PaginationResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                Pagesize = pageSize
            };
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> CreateAsync(User entity)
        {
            await _appDbContext.Users.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<User?> UpdateAsync(Guid id, User entity)
        {
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (existingUser is null)
            {
                return null;
            }
            existingUser.PhoneNumber = entity.PhoneNumber;
            existingUser.AvatarUrl = entity.AvatarUrl;
            existingUser.FullName = entity.FullName;

            if (!string.IsNullOrEmpty(entity.Password))
            {
                existingUser.Password = entity.Password;
            }

            await _appDbContext.SaveChangesAsync();
            return existingUser;
        }

        public async Task<User?> DeleteAsync(Guid id)
        {
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (existingUser is null)
            {
                return null;
            }
            _appDbContext.Users.Remove(existingUser);
            await _appDbContext.SaveChangesAsync();
            return existingUser;
        }

        public async Task<User?> GetByEmailOrPhoneAsync(string identifier)
        {
            return await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.Email == identifier || x.PhoneNumber == identifier);
        }
    }
}
