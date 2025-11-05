using ReEV.Common;
using ReEV.Service.Marketplace.DTOs;

namespace ReEV.Service.Marketplace.Services.Interfaces
{
    public interface IListingService
    {
        Task<ListingDTO> CreateListingAsync(Guid sellerId, CreateListingDTO dto);
        Task<ListingDTO?> GetListingByIdAsync(Guid id);
        Task<PaginationResult<ListingDTO>> GetAllListingsAsync(int page = 1, int pageSize = 10, string search = "");
        Task<ListingDTO?> UpdateListingAsync(Guid listingId, Guid sellerId, UpdateListingDTO dto);
        Task<ListingDTO?> VerifyListingAsync(Guid listingId, bool isVerified);
    }
}
