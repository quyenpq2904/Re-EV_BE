using Microsoft.AspNetCore.Identity;

namespace ReEV.Service.Auth.Helpers
{
    public static class PasswordHelper
    {
        private static readonly IPasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public static string HashPassword(string password)
        {
            string hashedPassword = _passwordHasher.HashPassword(null, password);
            return hashedPassword;
        }

        public static bool VerifyPassword(string passwordHashFromDb, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, passwordHashFromDb, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
