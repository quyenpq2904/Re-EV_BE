using ReEV.Common;
using ReEV.Service.Marketplace.DTOs;

namespace ReEV.Service.Marketplace.Services.Interfaces
{
    public interface IBidService
    {
        Task<BidDTO> CreateBidAsync(Guid bidderId, CreateBidDTO dto);
        Task<PaginationResult<BidDTO>> GetBidsAsync(Guid? listingId = null, int page = 1, int pageSize = 10);
    }
}

