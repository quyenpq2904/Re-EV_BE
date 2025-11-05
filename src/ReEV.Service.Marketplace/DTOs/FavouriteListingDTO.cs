namespace ReEV.Service.Marketplace.DTOs
{
    public class FavouriteListingDTO
    {
        public string? image { get; set; }
        public string Name { get; set; } = null!;
        public Guid listingId { get; set; }
    }
}

