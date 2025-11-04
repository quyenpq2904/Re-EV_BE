namespace ReEV.Service.Transaction.Models
{
    public sealed class User : Entity
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
        public ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();

        public User()
        {
        }
    }
}
