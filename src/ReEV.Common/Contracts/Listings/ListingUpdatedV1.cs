using ReEV.Common.Enums;

namespace ReEV.Common.Contracts.Listings
{
    public record ListingUpdatedV1(
        Guid ListingId,
        Guid SellerId,
        string Title,
        string Description,
        float Price,
        float? BiddingIncrements,
        ListingType ListingType,
        string Brand,
        string Model,
        int BatteryPercentage,
        int YearOfManufacture,
        Condition Condition,
        DateTimeOffset? AuctionStartTime,
        DateTimeOffset? AuctionEndTime,
        DateTimeOffset UpdatedAtUtc
    );
}

