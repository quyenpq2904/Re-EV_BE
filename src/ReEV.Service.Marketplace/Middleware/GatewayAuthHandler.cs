using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ReEV.Service.Marketplace.Middleware
{
    public class GatewayAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public GatewayAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Đọc user info từ custom headers mà Gateway đã forward
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
            var userRole = Request.Headers["X-User-Role"].FirstOrDefault();

            // Log để debug
            Logger.LogDebug("GatewayAuthHandler: X-User-Id={UserId}, X-User-Email={Email}, X-User-Role={Role}", 
                userId ?? "null", userEmail ?? "null", userRole ?? "null");

            // Nếu không có X-User-Id header, có nghĩa là Gateway chưa authenticate hoặc request không yêu cầu auth
            // Trong trường hợp này, không authenticate nhưng cũng không fail (để cho phép các endpoint public)
            if (string.IsNullOrEmpty(userId))
            {
                Logger.LogWarning("GatewayAuthHandler: Missing X-User-Id header. Request path: {Path}", Request.Path);
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Tạo claims từ headers
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("sub", userId), // JWT standard claim
                new Claim(JwtRegisteredClaimNames.Sub, userId) // JWT standard claim
            };

            if (!string.IsNullOrEmpty(userEmail))
            {
                claims.Add(new Claim(ClaimTypes.Email, userEmail));
                claims.Add(new Claim("email", userEmail));
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, userEmail));
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                claims.Add(new Claim("role", userRole));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/json";

            var message = new
            {
                message = "Unauthorized: Missing X-User-Id header. Please ensure you are accessing through the API Gateway with a valid JWT token."
            };

            var json = JsonSerializer.Serialize(message);
            return Response.WriteAsync(json);
        }
    }
}

