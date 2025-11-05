using Microsoft.EntityFrameworkCore;
using ReEV.Common;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;

namespace ReEV.Service.Marketplace.Repositories
{
    public class UserFavoriteRepository : IUserFavoriteRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserFavoriteRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<UserFavorite> CreateAsync(UserFavorite entity)
        {
            await _appDbContext.UserFavorites.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<UserFavorite?> DeleteAsync(Guid id)
        {
            var favorite = await _appDbContext.UserFavorites.FindAsync(id);
            if (favorite == null)
            {
                return null;
            }

            _appDbContext.UserFavorites.Remove(favorite);
            await _appDbContext.SaveChangesAsync();
            return favorite;
        }

        public async Task<PaginationResult<UserFavorite>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var favorites = _appDbContext.UserFavorites
                .Include(uf => uf.User)
                .Include(uf => uf.Listing)
                .AsQueryable();

            var totalCount = await favorites.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;

            var items = await favorites
                .OrderByDescending(uf => uf.CreatedAtUtc)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<UserFavorite>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                Pagesize = pageSize
            };
        }

        public async Task<UserFavorite?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.UserFavorites
                .Include(uf => uf.User)
                .Include(uf => uf.Listing)
                .FirstOrDefaultAsync(uf => uf.Id == id);
        }

        public Task<UserFavorite?> UpdateAsync(Guid id, UserFavorite entity)
        {
            throw new NotImplementedException();
        }

        public async Task<UserFavorite?> GetByUserIdAndListingIdAsync(Guid userId, Guid listingId)
        {
            return await _appDbContext.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ListingId == listingId);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid listingId)
        {
            return await _appDbContext.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.ListingId == listingId);
        }

        public async Task<PaginationResult<UserFavorite>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var favorites = _appDbContext.UserFavorites
                .Include(uf => uf.Listing)
                .Where(uf => uf.UserId == userId)
                .AsQueryable();

            var totalCount = await favorites.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;

            var items = await favorites
                .OrderByDescending(uf => uf.CreatedAtUtc)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<UserFavorite>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                Pagesize = pageSize
            };
        }
    }
}

