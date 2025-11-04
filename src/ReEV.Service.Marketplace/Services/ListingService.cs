using AutoMapper;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using ReEV.Service.Marketplace.Services.Interfaces;

namespace ReEV.Service.Marketplace.Services
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ListingService(
        IListingRepository listingRepository,
        IUserRepository userRepository,
        IMapper mapper)
        {
            _listingRepository = listingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ListingDTO> CreateListingAsync(Guid sellerId, CreateListingDTO dto)
        {
            var seller = await _userRepository.GetByIdAsync(sellerId);
            if (seller == null)
            {
                throw new KeyNotFoundException($"Seller with id {sellerId} not found.");
            }

            Listing listingEntity;
            if (dto.ListingType == ListingType.BUYNOW)
            {
                listingEntity = Listing.CreateBuyNowListing(
                    sellerId,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    dto.Images ?? Array.Empty<string>(),
                    dto.Brand,
                    dto.Model,
                    dto.BatteryPercentage,
                    dto.YearOfManufacture,
                    dto.Condition
                );
            }
            else if (dto.ListingType == ListingType.AUCTION)
            {
                if (!dto.AuctionStartTime.HasValue || !dto.AuctionEndTime.HasValue)
                    throw new ArgumentException("AuctionStartTime and AuctionEndTime must be provided for auction listing.");

                listingEntity = Listing.CreateAuctionListing(
                    sellerId,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    dto.BiddingIncrements.GetValueOrDefault(),
                    dto.Images ?? Array.Empty<string>(),
                    dto.Brand,
                    dto.Model,
                    dto.BatteryPercentage,
                    dto.YearOfManufacture,
                    dto.Condition,
                    dto.AuctionStartTime.Value,
                    dto.AuctionEndTime.Value
                );
            }
            else
            {
                throw new ArgumentException($"Unknown ListingType: {dto.ListingType}");
            }

            listingEntity.Seller = seller;
            await _listingRepository.CreateAsync(listingEntity);

            return _mapper.Map<ListingDTO>(listingEntity);
        }

        public async Task<ListingDTO?> GetListingByIdAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing is null)
            {
                return null; 
            }
           
            return _mapper.Map<ListingDTO>(listing);
        }
    }
}
