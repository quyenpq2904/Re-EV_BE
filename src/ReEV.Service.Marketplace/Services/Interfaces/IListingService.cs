using ReEV.Service.Marketplace.DTOs;

namespace ReEV.Service.Marketplace.Services.Interfaces
{
    public interface IListingService
    {
        Task<ListingDTO> CreateListingAsync(Guid sellerId, CreateListingDTO dto);
        Task<ListingDTO?> GetListingByIdAsync(Guid id);
    }
}
