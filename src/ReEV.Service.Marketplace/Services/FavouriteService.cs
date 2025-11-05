using ReEV.Common;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using ReEV.Service.Marketplace.Services.Interfaces;

namespace ReEV.Service.Marketplace.Services
{
    public class FavouriteService : IFavouriteService
    {
        private readonly IUserFavoriteRepository _userFavoriteRepository;
        private readonly IListingRepository _listingRepository;
        private readonly ILogger<FavouriteService> _logger;

        public FavouriteService(
            IUserFavoriteRepository userFavoriteRepository,
            IListingRepository listingRepository,
            ILogger<FavouriteService> logger)
        {
            _userFavoriteRepository = userFavoriteRepository;
            _listingRepository = listingRepository;
            _logger = logger;
        }

        public async Task<bool> AddFavouriteAsync(Guid userId, Guid listingId)
        {
            // Kiểm tra listing có tồn tại không
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                throw new KeyNotFoundException($"Listing with id {listingId} not found.");
            }

            // Kiểm tra đã có favourite chưa
            var exists = await _userFavoriteRepository.ExistsAsync(userId, listingId);
            if (exists)
            {
                throw new InvalidOperationException("Listing is already in favourites.");
            }

            // Tạo favourite mới
            var userFavorite = UserFavorite.Create(userId, listingId);
            await _userFavoriteRepository.CreateAsync(userFavorite);

            return true;
        }

        public async Task<PaginationResult<FavouriteListingDTO>> GetFavouritesAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var paginatedFavourites = await _userFavoriteRepository.GetByUserIdAsync(userId, page, pageSize);
            
            var favouriteDtos = paginatedFavourites.Items.Select(uf => new FavouriteListingDTO
            {
                image = uf.Listing?.Images != null && uf.Listing.Images.Length > 0 
                    ? uf.Listing.Images[0] 
                    : null,
                Name = uf.Listing?.Title ?? string.Empty,
                listingId = uf.Listing?.Id ?? Guid.Empty
            }).ToList();

            return new PaginationResult<FavouriteListingDTO>
            {
                Items = favouriteDtos,
                TotalCount = paginatedFavourites.TotalCount,
                TotalPages = paginatedFavourites.TotalPages,
                Page = paginatedFavourites.Page,
                Pagesize = paginatedFavourites.Pagesize
            };
        }

        public async Task<bool> DeleteFavouriteAsync(Guid userId, Guid? listingId = null, Guid? favouriteId = null)
        {
            UserFavorite? favorite = null;

            // Nếu có listing_id, tìm theo userId + listingId
            if (listingId.HasValue)
            {
                favorite = await _userFavoriteRepository.GetByUserIdAndListingIdAsync(userId, listingId.Value);
                if (favorite == null)
                {
                    throw new KeyNotFoundException($"Favourite with user_id {userId} and listing_id {listingId.Value} not found.");
                }
            }
            // Nếu có favourite_id, tìm theo favouriteId và kiểm tra userId
            else if (favouriteId.HasValue)
            {
                favorite = await _userFavoriteRepository.GetByIdAsync(favouriteId.Value);
                if (favorite == null)
                {
                    throw new KeyNotFoundException($"Favourite with id {favouriteId.Value} not found.");
                }
                // Kiểm tra userId có khớp không
                if (favorite.UserId != userId)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to delete this favourite.");
                }
            }
            else
            {
                throw new ArgumentException("Either listing_id or favourite_id must be provided.");
            }

            // Xóa favourite
            var deleted = await _userFavoriteRepository.DeleteAsync(favorite.Id);
            if (deleted == null)
            {
                throw new InvalidOperationException("Failed to delete favourite.");
            }

            return true;
        }
    }
}

