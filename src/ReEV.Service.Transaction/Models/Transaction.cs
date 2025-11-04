namespace ReEV.Service.Transaction.Models
{
    public sealed class Payment : Entity
    {
        public Guid OrderId { get; set; }
        public float Amount { get; set; }
        public TransactionStatus Status { get; set; }

        private Payment(Guid orderId, float amount)
        {
            OrderId = orderId;
            Amount = amount;
            Status = TransactionStatus.PENDING;
        }

        public static Payment Create(Guid orderId, float amount)
        {
            return new Payment(orderId, amount);
        }
    }
}
