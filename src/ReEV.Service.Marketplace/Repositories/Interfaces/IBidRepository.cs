using ReEV.Common;
using ReEV.Service.Marketplace.Models;

namespace ReEV.Service.Marketplace.Repositories.Interfaces
{
    public interface IBidRepository
    {
        Task<Bid> CreateAsync(Bid bid);
        Task<Bid?> GetByIdAsync(Guid id);
        Task<List<Bid>> GetBidsByListingIdAsync(Guid listingId);
        Task<List<Bid>> GetBidsByBidderIdAsync(Guid bidderId);
        Task<PaginationResult<Bid>> GetAllAsync(Guid? listingId = null, int page = 1, int pageSize = 10);
    }
}

