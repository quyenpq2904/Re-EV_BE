using AutoMapper;
using ReEV.Common;
using ReEV.Common.Enums;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Helpers;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using ReEV.Service.Marketplace.Services.Interfaces;

namespace ReEV.Service.Marketplace.Services
{
    public class BidService : IBidService
    {
        private readonly IBidRepository _bidRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<BidService> _logger;

        public BidService(
            IBidRepository bidRepository,
            IListingRepository listingRepository,
            IUserRepository userRepository,
            IMapper mapper,
            RabbitMQPublisher publisher,
            ILogger<BidService> logger)
        {
            _bidRepository = bidRepository;
            _listingRepository = listingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<BidDTO> CreateBidAsync(Guid bidderId, CreateBidDTO dto)
        {
            // Parse listing_id
            if (!Guid.TryParse(dto.listing_id, out Guid listingId))
            {
                throw new ArgumentException("Invalid listing_id format. Must be a valid GUID.");
            }

            // 1. Validate listing tồn tại
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                throw new KeyNotFoundException($"Listing with id {listingId} not found.");
            }

            // 2. Kiểm tra listing phải là AUCTION
            if (listing.ListingType != ListingType.AUCTION)
            {
                throw new InvalidOperationException("Bids can only be placed on AUCTION listings.");
            }

            // 3. Kiểm tra thời gian bid phải trong khoảng AuctionStartTime và AuctionEndTime
            var now = DateTimeOffset.UtcNow;
            if (!listing.AuctionStartTime.HasValue || !listing.AuctionEndTime.HasValue)
            {
                throw new InvalidOperationException("Auction start time and end time are not set for this listing.");
            }

            if (now < listing.AuctionStartTime.Value)
            {
                throw new InvalidOperationException($"Auction has not started yet. Start time: {listing.AuctionStartTime.Value}");
            }

            if (now > listing.AuctionEndTime.Value)
            {
                throw new InvalidOperationException($"Auction has ended. End time: {listing.AuctionEndTime.Value}");
            }

            // 4. Kiểm tra bid_amount có lớn hơn bước nhảy của listing hay không
            if (!listing.BiddingIncrements.HasValue)
            {
                throw new InvalidOperationException("Bidding increments is not set for this listing.");
            }

            // Lấy bid cao nhất hiện tại hoặc giá khởi điểm
            var highestBid = await _bidRepository.GetBidsByListingIdAsync(listingId);
            var currentHighestAmount = highestBid.Count > 0 ? highestBid[0].BidAmount : listing.Price;

            var minimumBidAmount = currentHighestAmount + listing.BiddingIncrements.Value;
            if (dto.bid_ammount < minimumBidAmount)
            {
                throw new ArgumentException(
                    $"Bid amount must be at least {minimumBidAmount}. Current highest bid: {currentHighestAmount}, Bidding increment: {listing.BiddingIncrements.Value}");
            }

            // 5. Kiểm tra balance - lockedBalance của user có >= bid_amount hay không
            var user = await _userRepository.GetByIdAsync(bidderId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {bidderId} not found.");
            }

            var availableBalance = user.Balance - user.LockedBalance;
            if (availableBalance < dto.bid_ammount)
            {
                throw new InvalidOperationException(
                    $"Insufficient balance. Available balance: {availableBalance}, Required: {dto.bid_ammount}");
            }

            // 6. Tạo bid và đồng thời lockedBalance += bid_amount
            var bid = Bid.Create(bidderId, listingId, dto.bid_ammount);
            await _bidRepository.CreateAsync(bid);

            // Update locked balance
            user.LockedBalance += dto.bid_ammount;
            var updatedUser = await _userRepository.UpdateAsync(user.Id, user);
            if (updatedUser == null)
            {
                throw new InvalidOperationException("Failed to update user locked balance.");
            }

            // Publish event để đồng bộ Balance và LockedBalance với AuthService
            await BalanceHelper.PublishBalanceUpdateEventAsync(updatedUser, _publisher, _logger);

            _logger.LogInformation(
                "Bid created successfully. BidId: {BidId}, BidderId: {BidderId}, ListingId: {ListingId}, Amount: {Amount}, NewLockedBalance: {LockedBalance}",
                bid.Id, bidderId, listingId, dto.bid_ammount, updatedUser.LockedBalance);

            return new BidDTO
            {
                Id = bid.Id,
                BidderId = bid.BidderId,
                ListingId = bid.ListingId,
                BidAmount = bid.BidAmount,
                CreatedAtUtc = bid.CreatedAtUtc
            };
        }

        public async Task<PaginationResult<BidDTO>> GetBidsAsync(Guid? listingId = null, int page = 1, int pageSize = 10)
        {
            var paginatedBids = await _bidRepository.GetAllAsync(listingId, page, pageSize);
            var bidDtos = _mapper.Map<List<BidDTO>>(paginatedBids.Items);

            return new PaginationResult<BidDTO>
            {
                Items = bidDtos,
                TotalCount = paginatedBids.TotalCount,
                TotalPages = paginatedBids.TotalPages,
                Page = paginatedBids.Page,
                Pagesize = paginatedBids.Pagesize
            };
        }
    }
}

