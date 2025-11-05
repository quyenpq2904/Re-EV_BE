using AutoMapper;
using Microsoft.AspNetCore.Http;
using ReEV.Common;
using ReEV.Common.Contracts.Listings;
using ReEV.Common.Enums;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using ReEV.Service.Marketplace.Services.Interfaces;
using System.IO;
using System.Net.Http.Headers;

namespace ReEV.Service.Marketplace.Services
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<ListingService> _logger;

        public ListingService(
        IListingRepository listingRepository,
        IUserRepository userRepository,
        IMapper mapper,
        RabbitMQPublisher publisher,
        ILogger<ListingService> logger)
        {
            _listingRepository = listingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ListingDTO> CreateListingAsync(Guid sellerId, CreateListingDTO dto)
        {
            var seller = await _userRepository.GetByIdAsync(sellerId);
            if (seller == null)
            {
                throw new KeyNotFoundException($"Seller with id {sellerId} not found.");
            }

            // Validate và xử lý ảnh
            var imageUrls = await ProcessImagesAsync(dto.Images);

            Listing listingEntity;

            // Tạo listing dựa trên listing_type
            if (dto.ListingType == ListingType.BUYNOW)
            {
                listingEntity = Listing.CreateBuyNowListing(
                    sellerId,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    imageUrls,
                    dto.Brand,
                    dto.Model,
                    dto.BatteryPercentage,
                    dto.YearOfManufacture,
                    dto.Condition
                );
            }
            else if (dto.ListingType == ListingType.AUCTION)
            {
                // Validate các fields required cho auction
                if (!dto.BiddingIncrements.HasValue)
                {
                    throw new ArgumentException("BiddingIncrements is required for AUCTION listing.");
                }

                if (!dto.AuctionStartTime.HasValue || !dto.AuctionEndTime.HasValue)
                {
                    throw new ArgumentException("AuctionStartTime and AuctionEndTime are required for AUCTION listing.");
                }

                listingEntity = Listing.CreateAuctionListing(
                    sellerId,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    dto.BiddingIncrements.Value,
                    imageUrls,
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
                throw new ArgumentException($"Invalid ListingType: {dto.ListingType}. Valid values are 0 (BUYNOW) or 1 (AUCTION).");
            }

            listingEntity.Seller = seller;
            await _listingRepository.CreateAsync(listingEntity);

            // Publish event để đồng bộ với transaction service
            await PublishListingCreatedEvent(listingEntity);

            return _mapper.Map<ListingDTO>(listingEntity);
        }

        // TODO: Bước 3, 4, 5 - Upload ảnh lên storage và lấy URLs
        // Hiện tại chỉ validate file types và sizes
        private async Task<string[]> ProcessImagesAsync(IFormFile[]? images)
        {
            if (images == null || images.Length == 0)
            {
                return Array.Empty<string>();
            }

            // Bước 1: Validate file types (chỉ cho phép jpg, jpeg, png, webp)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            const long maxFileSize = 5 * 1024 * 1024; // 5MB

            foreach (var image in images)
            {
                // Validate file extension
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException(
                        $"File '{image.FileName}' has invalid extension. Allowed extensions: {string.Join(", ", allowedExtensions)}");
                }

                // Validate content type
                if (string.IsNullOrEmpty(image.ContentType) || !allowedContentTypes.Contains(image.ContentType.ToLowerInvariant()))
                {
                    throw new ArgumentException(
                        $"File '{image.FileName}' has invalid content type. Allowed types: {string.Join(", ", allowedContentTypes)}");
                }

                // Bước 2: Validate file size (max 5MB mỗi ảnh)
                if (image.Length > maxFileSize)
                {
                    throw new ArgumentException(
                        $"File '{image.FileName}' exceeds maximum file size of {maxFileSize / (1024 * 1024)}MB. File size: {image.Length / (1024 * 1024)}MB");
                }

                if (image.Length == 0)
                {
                    throw new ArgumentException($"File '{image.FileName}' is empty.");
                }
            }

            _logger.LogInformation("Image validation passed. Total images: {ImageCount}", images.Length);

            // TODO: Bước 3 - Upload ảnh lên storage (S3, Azure Blob, hoặc local storage)
            // TODO: Bước 4 - Lấy URLs sau khi upload thành công
            // TODO: Bước 5 - Lưu URLs vào database
            // Hiện tại tạm thời trả về empty array, sẽ implement sau
            
            await Task.CompletedTask;
            return Array.Empty<string>();
        }

        private async Task PublishListingCreatedEvent(Listing listing)
        {
            try
            {
                var listingEvent = new ListingCreatedV1(
                    listing.Id,
                    listing.SellerId,
                    listing.Title,
                    listing.Description,
                    listing.Price,
                    listing.BiddingIncrements,
                    listing.ListingType,
                    listing.Brand,
                    listing.Model,
                    listing.BatteryPercentage,
                    listing.YearOfManufacture,
                    listing.Condition,
                    listing.AuctionStartTime,
                    listing.AuctionEndTime,
                    listing.CreatedAtUtc
                );

                await _publisher.PublishListingCreatedAsync(listingEvent);
                _logger.LogInformation("Listing created event published successfully. ListingId: {ListingId}", listing.Id);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không làm fail request
                // Event sẽ được retry hoặc xử lý sau
                _logger.LogError(ex, "Failed to publish listing created event. ListingId: {ListingId}", listing.Id);
            }
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

        public async Task<PaginationResult<ListingDTO>> GetAllListingsAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var paginatedListings = await _listingRepository.GetAllAsync(page, pageSize, search);
            var listingDtos = _mapper.Map<List<ListingDTO>>(paginatedListings.Items);
            
            return new PaginationResult<ListingDTO>
            {
                Items = listingDtos,
                TotalCount = paginatedListings.TotalCount,
                TotalPages = paginatedListings.TotalPages,
                Page = paginatedListings.Page,
                Pagesize = paginatedListings.Pagesize
            };
        }

        public async Task<ListingDTO?> UpdateListingAsync(Guid listingId, Guid sellerId, UpdateListingDTO dto)
        {
            var existingListing = await _listingRepository.GetByIdAsync(listingId);
            if (existingListing == null)
            {
                return null;
            }

            // Kiểm tra quyền: chỉ seller mới được update listing của mình
            if (existingListing.SellerId != sellerId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this listing.");
            }

            // Xử lý ảnh nếu có
            string[]? imageUrls = null;
            if (dto.Images != null && dto.Images.Length > 0)
            {
                imageUrls = await ProcessImagesAsync(dto.Images);
            }
            else
            {
                // Giữ nguyên ảnh cũ nếu không có ảnh mới
                imageUrls = existingListing.Images;
            }

            // Cập nhật các thuộc tính
            existingListing.Title = dto.Title;
            existingListing.Description = dto.Description;
            existingListing.Price = dto.Price;
            existingListing.Images = imageUrls;
            existingListing.BatteryPercentage = dto.BatteryPercentage;
            existingListing.YearOfManufacture = dto.YearOfManufacture;
            existingListing.Condition = dto.Condition;
            existingListing.UpdatedAtUtc = DateTimeOffset.UtcNow;

            var updatedListing = await _listingRepository.UpdateAsync(listingId, existingListing);
            if (updatedListing == null)
            {
                return null;
            }

            // Publish event để đồng bộ với transaction service
            await PublishListingUpdatedEvent(updatedListing);

            return _mapper.Map<ListingDTO>(updatedListing);
        }

        public async Task<ListingDTO?> VerifyListingAsync(Guid listingId, bool isVerified)
        {
            var listing = await _listingRepository.VerifyListingAsync(listingId, isVerified);
            if (listing == null)
            {
                return null;
            }

            // Publish event để đồng bộ với transaction service
            await PublishListingUpdatedEvent(listing);

            _logger.LogInformation("Listing {Action}. ListingId: {ListingId}", 
                isVerified ? "verified" : "unverified", listingId);

            return _mapper.Map<ListingDTO>(listing);
        }

        private async Task PublishListingUpdatedEvent(Listing listing)
        {
            try
            {
                var listingEvent = new ListingUpdatedV1(
                    listing.Id,
                    listing.SellerId,
                    listing.Title,
                    listing.Description,
                    listing.Price,
                    listing.BiddingIncrements,
                    listing.ListingType,
                    listing.Brand,
                    listing.Model,
                    listing.BatteryPercentage,
                    listing.YearOfManufacture,
                    listing.Condition,
                    listing.AuctionStartTime,
                    listing.AuctionEndTime,
                    listing.UpdatedAtUtc
                );

                await _publisher.PublishListingUpdatedAsync(listingEvent);
                _logger.LogInformation("Listing updated event published successfully. ListingId: {ListingId}", listing.Id);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không làm fail request
                // Event sẽ được retry hoặc xử lý sau
                _logger.LogError(ex, "Failed to publish listing updated event. ListingId: {ListingId}", listing.Id);
            }
        }
    }
}
