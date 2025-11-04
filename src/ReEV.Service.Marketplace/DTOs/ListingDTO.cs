namespace ReEV.Service.Marketplace.DTOs
{
    public class ListingDTO
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public float Price { get; set; }
        public float? BiddingIncrements { get; set; }
        public float? EndPrice { get; set; }
        public ListingType ListingType { get; set; }
        public bool IsVerified { get; set; }
        public string[]? Images { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int BatteryPercentage { get; set; }
        public int YearOfManufacture { get; set; }
        public Condition Condition { get; set; }
        public DateTimeOffset? AuctionStartTime { get; set; }
        public DateTimeOffset? AuctionEndTime { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }    // nếu entity có CreatedAt
        public DateTimeOffset? UpdatedAtUtc { get; set; }   // nếu có
    }
}
