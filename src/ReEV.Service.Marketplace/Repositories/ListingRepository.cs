using Microsoft.EntityFrameworkCore;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;

namespace ReEV.Service.Marketplace.Repositories
{
    public class ListingRepository : IListingRepository
    {
        private readonly AppDbContext _appDbContext;
        public ListingRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Listing> CreateAsync(Listing entity)
        {
            await _appDbContext.Listings.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public Task<Listing?> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResult<Listing>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            throw new NotImplementedException();
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Listings.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Listing?> UpdateAsync(Guid id, Listing entity)
        {
            throw new NotImplementedException();
        }
    }
}
