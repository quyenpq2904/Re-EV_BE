using Microsoft.EntityFrameworkCore;
using ReEV.Common;
using ReEV.Common.Enums;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using System.Linq;

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

        public async Task<PaginationResult<Listing>> GetAllAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var listings = _appDbContext.Listings
                .Include(l => l.Seller)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLowerInvariant();
                listings = listings.Where(l =>
                    l.Title.ToLower().Contains(search) ||
                    l.Description.ToLower().Contains(search) ||
                    l.Brand.ToLower().Contains(search) ||
                    l.Model.ToLower().Contains(search));
            }

            var totalCount = await listings.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;

            var items = await listings
                .OrderByDescending(l => l.CreatedAtUtc)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<Listing>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                Pagesize = pageSize
            };
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Listings
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Listing?> UpdateAsync(Guid id, Listing entity)
        {
            var existingListing = await _appDbContext.Listings.FindAsync(id);
            if (existingListing == null)
            {
                return null;
            }

            // Update các thuộc tính có thể thay đổi
            existingListing.Title = entity.Title;
            existingListing.Description = entity.Description;
            existingListing.Price = entity.Price;
            existingListing.Images = entity.Images;
            existingListing.BatteryPercentage = entity.BatteryPercentage;
            existingListing.YearOfManufacture = entity.YearOfManufacture;
            existingListing.Condition = entity.Condition;
            existingListing.UpdatedAtUtc = DateTimeOffset.UtcNow;

            // Nếu là auction listing, có thể update bidding increments và auction times
            if (entity.ListingType == ListingType.AUCTION)
            {
                existingListing.BiddingIncrements = entity.BiddingIncrements;
                existingListing.AuctionStartTime = entity.AuctionStartTime;
                existingListing.AuctionEndTime = entity.AuctionEndTime;
            }

            await _appDbContext.SaveChangesAsync();
            return existingListing;
        }

        public async Task<Listing?> VerifyListingAsync(Guid id, bool isVerified)
        {
            var listing = await _appDbContext.Listings.FindAsync(id);
            if (listing == null)
            {
                return null;
            }

            listing.IsVerified = isVerified;
            listing.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _appDbContext.SaveChangesAsync();
            return listing;
        }
    }
}
