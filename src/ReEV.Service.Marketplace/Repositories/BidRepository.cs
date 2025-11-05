using Microsoft.EntityFrameworkCore;
using ReEV.Common;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;

namespace ReEV.Service.Marketplace.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly AppDbContext _appDbContext;

        public BidRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Bid> CreateAsync(Bid bid)
        {
            await _appDbContext.Bids.AddAsync(bid);
            await _appDbContext.SaveChangesAsync();
            return bid;
        }

        public async Task<Bid?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Bids
                .Include(b => b.Bidder)
                .Include(b => b.Listing)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Bid>> GetBidsByListingIdAsync(Guid listingId)
        {
            return await _appDbContext.Bids
                .Include(b => b.Bidder)
                .Where(b => b.ListingId == listingId)
                .OrderByDescending(b => b.BidAmount)
                .ThenByDescending(b => b.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<Bid>> GetBidsByBidderIdAsync(Guid bidderId)
        {
            return await _appDbContext.Bids
                .Include(b => b.Listing)
                .Where(b => b.BidderId == bidderId)
                .OrderByDescending(b => b.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<PaginationResult<Bid>> GetAllAsync(Guid? listingId = null, int page = 1, int pageSize = 10)
        {
            var bids = _appDbContext.Bids
                .Include(b => b.Bidder)
                .Include(b => b.Listing)
                .AsQueryable();

            // Filter by listing_id nếu có
            if (listingId.HasValue)
            {
                bids = bids.Where(b => b.ListingId == listingId.Value);
            }

            // Sort theo thứ tự mới nhất đến cũ nhất (CreatedAtUtc desc)
            bids = bids.OrderByDescending(b => b.CreatedAtUtc);

            var totalCount = await bids.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;

            var items = await bids.Skip(skip).Take(pageSize).ToListAsync();

            return new PaginationResult<Bid>
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

