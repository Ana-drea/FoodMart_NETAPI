using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace MiniMart.Services
{
    public class CheckIsAdminService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckIsAdminService(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <summary>
        /// Check if input user (nullable) and email is admin
        /// </summary>
        /// <param name="user"> nullable IdentityUser </param>
        /// <param name="email"> user email </param>
        /// <returns> if is admin return true, else return false </returns>
        public async Task<bool> CheckIsAdminAsync(IdentityUser user, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            // Get admin email from config
            var adminEmail = _configuration["AdminSettings:Email"];

            // match input email with admin email
            if (!string.IsNullOrEmpty(adminEmail) &&
                string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // If user is provided, check if user has "Admin" role
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return true;
            }

            return false;
        }
    }
}
