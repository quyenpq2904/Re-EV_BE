using Microsoft.IdentityModel.Tokens;
using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Models;
using ReEV.Service.Auth.Repositories.Interfaces;
using ReEV.Service.Auth.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ReEV.Service.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;

        public TokenService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepo, IUserRepository userRepository)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepo;
            _userRepository = userRepository;
        }

        public async Task<TokenResponseDTO> GenerateTokensAsync(User user)
        {
            string accessToken = GenerateAccessToken(user);
            string refreshToken = GenerateRefreshToken();

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreationDate = DateTimeOffset.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryDays"])),
            });

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDTO> RenewToken(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal?.Identity?.Name == null)
            {
                throw new SecurityTokenException("Invalid access token");
            }
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (userId == null)
            {
                throw new SecurityTokenException("Invalid access token: Missing 'sub' claim");
            }
            var token = await _refreshTokenRepository.GetRefreshTokenByUserIdAndToken(Guid.Parse(userId), refreshToken);

            if (token == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (token.ExpiryDate <= DateTimeOffset.UtcNow)
            {
                throw new SecurityTokenException("Refresh token expired");
            }

            if(token.IsActive == false)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiryDate = DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryMinutes"]));

            await _refreshTokenRepository.UpdateAsync(token.Id);
            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiryDate = expiryDate
            });

            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpiryMinutes"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessExpiry,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"])
            ),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch (Exception ex) 
            {
                return null;
            }

        }
    }
}
