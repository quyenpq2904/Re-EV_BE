namespace ReEV.Service.Auth.DTOs
{
    public class LoginResponseDTO
    {
        public TokenResponseDTO Token { get; set; }
        public UserDTO User { get; set; }
    }
}
