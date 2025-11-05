namespace ReEV.Service.Auth.Models
{
    public sealed class User : Entity
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string? AvatarUrl { get; set; } = null;
        public float Balance { get; set; } = 0;
        public float LockedBalance { get; set; } = 0;
        public UserRole Role { get; set; } = UserRole.MEMBER;
        public UserStatus Status { get; set; } = UserStatus.UNVERIFIED;
        public ICollection<RefreshToken> Sessions { get; set; } = new List<RefreshToken>();

        public User()
        {
        }
    }
}
