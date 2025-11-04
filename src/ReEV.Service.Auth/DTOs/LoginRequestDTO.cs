using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Auth.DTOs
{
    public class LoginRequestDTO
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
