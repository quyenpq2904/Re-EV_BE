namespace ReEV.Service.Marketplace.Models
{
    public sealed class Review : Entity
    {
        public Guid OrderId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; } = null;
        public User Reviewer { get; set; } = null!;
        public User Reviewee { get; set; } = null!;

        private Review(Guid orderId, Guid reviewerId, Guid revieweeId, int rating, string? comment)
        {
            OrderId = orderId;
            ReviewerId = reviewerId;
            RevieweeId = revieweeId;
            Rating = rating;
            Comment = comment;
        }

        public static Review Create(Guid orderId, Guid reviewerId, Guid revieweeId, int rating, string? comment)
        {
            return new Review(orderId, reviewerId, revieweeId, rating, comment);
        }
    }
}
