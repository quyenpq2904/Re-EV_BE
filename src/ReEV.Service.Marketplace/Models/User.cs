namespace ReEV.Service.Marketplace.Models
{
    public sealed class User : Entity
    {
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; }
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
        public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<Complaint> ComplaintsMade { get; set; } = new List<Complaint>();
        public ICollection<Complaint> ComplaintsReceived { get; set; } = new List<Complaint>();

        public User()
        {
        }
    }
}
