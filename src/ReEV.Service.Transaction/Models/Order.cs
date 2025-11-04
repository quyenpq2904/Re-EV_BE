namespace ReEV.Service.Transaction.Models
{
    public sealed class Order : Entity
    {
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public float TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public OrderStatus Status { get; set; }
        public User Buyer { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        private Order(Guid buyerId, Guid sellerId, float totalAmount, string shippingAddress, string receiverName, string receiverPhone)
        {
            BuyerId = buyerId;
            SellerId = sellerId;
            TotalAmount = totalAmount;
            ShippingAddress = shippingAddress;
            ReceiverName = receiverName;
            ReceiverPhone = receiverPhone;
        }

        public static Order Create(Guid buyerId, Guid sellerId, float totalAmount, string shippingAddress, string receiverName, string receiverPhone)
        {
            return new Order(buyerId, sellerId, totalAmount, shippingAddress, receiverName, receiverPhone);
        }
    }
}
