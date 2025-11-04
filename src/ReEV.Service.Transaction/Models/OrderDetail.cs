namespace ReEV.Service.Transaction.Models
{
    public sealed class OrderDetail : Entity
    {
        public Guid OrderId { get; set; }
        public Guid ListingId { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public Order Order { get; set; } = null!;
        public Listing Listing { get; set; } = null!;

        private OrderDetail(Guid orderId, Guid listingId, float price, int quantity)
        {
            OrderId = orderId;
            ListingId = listingId;
            Price = price;
            Quantity = quantity;
        }

        public static OrderDetail Create(Guid orderId, Guid listingId, float price, int quantity)
        {
            return new OrderDetail(orderId, listingId, price, quantity);
        }
    }
}
