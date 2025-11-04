namespace ReEV.Service.Marketplace.Models
{
    public sealed class Bid : Entity
    {
        public Guid BidderId { get; set; }
        public Guid ListingId { get; set; }
        public float BidAmount { get; set; }
        public User Bidder { get; set; } = null!;
        public Listing Listing { get; set; } = null!;

        private Bid(Guid bidderId, Guid listingId, float bidAmount)
        {
            BidderId = bidderId;
            ListingId = listingId;
            BidAmount = bidAmount;
        }

        public static Bid Create(Guid bidderId, Guid listingId, float bidAmount)
        {
            return new Bid(bidderId, listingId, bidAmount);
        }
    }
}
