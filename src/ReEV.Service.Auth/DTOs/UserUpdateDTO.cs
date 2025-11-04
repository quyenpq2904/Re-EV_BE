using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Auth.DTOs
{
    public class UserUpdateDTO
    {
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string FullName { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
