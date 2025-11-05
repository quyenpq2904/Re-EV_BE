namespace ReEV.Service.Marketplace.DTOs
{
    public class FavouriteListingDTO
    {
        public string? image { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public Guid listing_id { get; set; }
    }
}

