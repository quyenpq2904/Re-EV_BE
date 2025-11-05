namespace ReEV.Service.Marketplace.DTOs
{
    public class BidDTO
    {
        public Guid Id { get; set; }
        public Guid BidderId { get; set; }
        public Guid ListingId { get; set; }
        public float BidAmount { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}

