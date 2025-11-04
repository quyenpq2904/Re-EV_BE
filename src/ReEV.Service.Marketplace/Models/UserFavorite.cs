namespace ReEV.Service.Marketplace.Models
{
    public sealed class UserFavorite : Entity
    {
        public Guid UserId { get; set; }
        public Guid ListingId { get; set; }
        public User User { get; set; } = null!;
        public Listing Listing { get; set; } = null!;

        private UserFavorite(Guid userId, Guid listingId)
        {
            UserId = userId;
            ListingId = listingId;
        }

        public static UserFavorite Create(Guid userId, Guid listingId)
        {
            return new UserFavorite(userId, listingId);
        }
    }
}
