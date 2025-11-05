using AutoMapper;
using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Exceptions;
using ReEV.Service.Auth.Helpers;
using ReEV.Service.Auth.Repositories.Interfaces;
using ReEV.Service.Auth.Services.Interfaces;

namespace ReEV.Service.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IUserService userService,
            ITokenService tokenService,
            IMapper mapper)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _userService = userService;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<UserDTO> Register(UserCreateDTO registerDto)
        {
            var errors = new Dictionary<string, string>();

            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
            {
                errors["email"] = "Email already exists.";
            }

            var existingUserByPhone = await _userRepository.GetByPhoneNumberAsync(registerDto.PhoneNumber);
            if (existingUserByPhone != null)
            {
                errors["phone_number"] = "Phone number already exists.";
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }

            string hashedPassword = PasswordHelper.HashPassword(registerDto.Password);
            registerDto.Password = hashedPassword;
            var newUserDto = await _userService.CreateUser(registerDto);

            return newUserDto;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginDto)
        {
            var user = await _userRepository.GetByEmailOrPhoneAsync(loginDto.Identifier);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (!PasswordHelper.VerifyPassword(user.Password, loginDto.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (user.Status == UserStatus.BANNED)
            {
                throw new UnauthorizedAccessException("Account is banned.");
            }

            var tokens = await _tokenService.GenerateTokensAsync(user);

            return new LoginResponseDTO
            {
                Token = tokens,
                User = _mapper.Map<UserDTO>(user)
            };
        }
    }
}
