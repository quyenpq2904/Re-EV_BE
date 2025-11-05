using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Auth.DTOs
{
    public class UserUpdateDTO
    {
        [MaxLength(100)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
