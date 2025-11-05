using ReEV.Common;
using ReEV.Service.Marketplace.DTOs;

namespace ReEV.Service.Marketplace.Services.Interfaces
{
    public interface IFavouriteService
    {
        Task<bool> AddFavouriteAsync(Guid userId, Guid listingId);
        Task<PaginationResult<FavouriteListingDTO>> GetFavouritesAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<bool> DeleteFavouriteAsync(Guid userId, Guid listingId);
    }
}

