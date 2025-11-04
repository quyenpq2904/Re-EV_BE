namespace ReEV.Service.Transaction.Models
{
    public sealed class Listing : Entity
    {
        public string Title { get; set; }
        public string MainImage { get; set; }
        public Guid SellerId { get; set; }
        public User Seller { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        private Listing(Guid id, string title, string mainImage, Guid sellerId)
        {
            Id = id;
            Title = title;
            MainImage = mainImage;
            SellerId = sellerId;
        }

        public static Listing Create(Guid listingId, string title, string mainImage, Guid sellerId)
        {
            return new Listing(listingId, title, mainImage, sellerId);
        }
    }
}
