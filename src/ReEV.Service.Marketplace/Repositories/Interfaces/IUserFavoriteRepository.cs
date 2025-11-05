using ReEV.Common;
using ReEV.Common.Interfaces;
using ReEV.Service.Marketplace.Models;

namespace ReEV.Service.Marketplace.Repositories.Interfaces
{
    public interface IUserFavoriteRepository : IRepository<UserFavorite>
    {
        Task<UserFavorite?> GetByUserIdAndListingIdAsync(Guid userId, Guid listingId);
        Task<bool> ExistsAsync(Guid userId, Guid listingId);
        Task<PaginationResult<UserFavorite>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    }
}

