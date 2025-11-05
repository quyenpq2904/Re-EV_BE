using System.Security.Claims;

namespace ReEV.Service.Marketplace.Helpers
{
    public static class UserHelper
    {
        /// <summary>
        /// Lấy User ID từ claims (được populate từ Gateway headers)
        /// </summary>
        public static Guid? GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            var userIdClaim = user.FindFirst("sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }

        /// <summary>
        /// Lấy User Role từ claims (được populate từ Gateway headers)
        /// </summary>
        public static string? GetUserRole(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            return user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value;
        }

        /// <summary>
        /// Kiểm tra user có role ADMIN không
        /// </summary>
        public static bool IsAdmin(ClaimsPrincipal? user)
        {
            var role = GetUserRole(user);
            return role == "ADMIN";
        }
    }
}

