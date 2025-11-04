namespace ReEV.Service.Auth.Models
{
    public sealed class RefreshToken
    {

        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public RefreshToken()
        {
        }
    }
}
