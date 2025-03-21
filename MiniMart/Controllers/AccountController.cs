﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using MiniMart.Models;
using MiniMart.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;
using System.Configuration;
using System.Security;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly CheckIsAdminService _checkIsAdminService;
        private readonly RsaService _rsaService;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, CheckIsAdminService checkIsAdminService, RsaService rsaService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _checkIsAdminService = checkIsAdminService;
            _rsaService = rsaService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // RSA decrypt the password
            var encryptedBytes = Convert.FromBase64String(request.Password);
            var plainPassword = _rsaService.Decrypt(encryptedBytes);


            // Call Identity sign in function
            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                plainPassword,
                true, // Whether to remember sign-in state
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Create JWT Token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1), // Set token expiration
                    // Sign JWT Token
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                return Ok(new { token = tokenString });
            }
            else
            {
                // 401: invalid username or password
                return Unauthorized(new { message = "Invalid credentials" }); 
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // RSA decrypt the password
            var encryptedBytes = Convert.FromBase64String(request.Password);
            var plainPassword = _rsaService.Decrypt(encryptedBytes);

            // Create user
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            // Save user to the database
            var result = await _userManager.CreateAsync(user, plainPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // _logger.LogInformation("User created a new account with password.");

            // Generate email confirmation token
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Convert the token to Base64 encoding
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Build email confirmation link
            var callbackUrl = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            // Send email confirmation link
            await _emailSender.SendEmailAsync(request.Email, "Confirm your email",
                $"Please confirm your email by clicking <a href='{callbackUrl}'>here</a>.");

            // Return confirmation link to frontend
            return Ok();
        }

        // API for email confirmation
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("User ID and Code are required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Decode Base64 token
            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            // Confirm email
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
            if (!result.Succeeded)
            {
                return BadRequest("Email confirmation failed.");
            }

            return Ok("Email confirmed successfully.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data." });
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Check if the user is admin
            if (await _checkIsAdminService.CheckIsAdminAsync(user, request.Email))
            {
                return BadRequest(new { Message = "Reset password is not allowed for an admin user." });
            }

            // Generate a password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token (base64 encoding for URL-safe transmission)
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            // Base URL from server configuration
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; // Build base URL from the request context
            var resetLink = $"{baseUrl}/reset-password?email={request.Email}&token={encodedToken}";


            // Here, you can send the `resetLink` to the user via email.
            return Ok(new { Message = "Password reset link has been sent.", ResetLink = resetLink });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data." });
            }

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(resetRequest.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // RSA decrypt the password
            var encryptedBytes = Convert.FromBase64String(resetRequest.NewPassword);
            var plainPassword = _rsaService.Decrypt(encryptedBytes);

            // Decode the token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));

            // Attempt to reset the password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, plainPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password reset successful." });
            }

            // Return validation errors
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Password reset failed.", Errors = errors });
        }
		
		[Authorize] // Ensure the user is logged in
        [HttpPost("change-password")] // Change password when user is still logged in
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data." });
            }

            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not logged in." });
            }

            // Check if the user is admin
            if (await _checkIsAdminService.CheckIsAdminAsync(user, user.Email))
            {
                return BadRequest(new { Message = "Change password is not allowed for an admin user." });
            }

            // RSA decrypt the passwords
            var currenteEncryptedBytes = Convert.FromBase64String(request.CurrentPassword);
            var currentPlainPassword = _rsaService.Decrypt(currenteEncryptedBytes);

            var newEncryptedBytes = Convert.FromBase64String(request.NewPassword);
            var newPlainPassword = _rsaService.Decrypt(newEncryptedBytes);
            // Attempt to change the password
            var result = await _userManager.ChangePasswordAsync(user, currentPlainPassword, newPlainPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password changed successfully." });
            }

            // Return validation errors
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Failed to change password.", Errors = errors });
        }


        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data." });
            }

            // use User to get currently user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Check if the user is admin
            if (await _checkIsAdminService.CheckIsAdminAsync(user, user.Email))
            {
                return BadRequest(new { Message = "Change email is not allowed for an admin user." });
            }

            // Generate an email change token
            var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

            // Encode the token (base64 encoding for URL-safe transmission)
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(changeEmailToken));

            // Build email confirmation link
            var callbackUrl = Url.Action(
                action: nameof(ConfirmChangeEmail),
                controller: "Account",
                values: new { userId = user.Id, newEmail= request.NewEmail, token = encodedToken },
                protocol: Request.Scheme);

            // Send email confirmation link
            await _emailSender.SendEmailAsync(request.NewEmail, "Confirm your new email",
                $"Please confirm your new email by clicking <a href='{callbackUrl}'>here</a>.");

            return Ok(new { Message = "Email change confirmation link has been sent."});
        }

        [HttpGet("confirm-change-email")]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newEmail) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Message = "Invalid parameters." });
            }

            // Decode the token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            // Find user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Check if the new email is already taken
            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "This email is already in use." });
            }
			
            // Confirm email change
            var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Failed to change email.", Errors = result.Errors });
            }

            // Update UserName to match the new email
            user.UserName = newEmail; // Update UserName to the new email
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to update username.", Errors = updateResult.Errors });
            }

            return Ok(new { Message = "Email has been changed successfully." });
        }

        [HttpGet("current-info")]
        public async Task<IActionResult> GetCurrentEmail()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            // Get user role
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { Email = user.Email, PhoneNumber=user.PhoneNumber, Roles = roles });
        }
		
		[Authorize]
        [HttpPost("add-phone-number")]
        public async Task<IActionResult> AddPhoneNumber([FromBody] AddPhoneNumberRequest request)
        {
            // Validate the phone number format
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !Regex.IsMatch(request.PhoneNumber, @"^\d{10}$"))
            {
                return BadRequest(new { Message = "Invalid phone number. It must be exactly 10 digits." });
            }

            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not found or not logged in." });
            }

            // Check if the user is admin
            if (await _checkIsAdminService.CheckIsAdminAsync(user, user.Email))
            {
                return BadRequest(new { Message = "Add/change phone number is not allowed for an admin user." });
            }

            // Update the phone number
            user.PhoneNumber = request.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Phone number updated successfully." });
            }

            // Handle errors during update
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Failed to update phone number.", Errors = errors });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out the current user
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return Ok(new { Message = "User has been logged out successfully." });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                // _logger.LogError(ex, "An error occurred while logging out.");
                return StatusCode(500, new { Message = "Logout successful!", Error = ex.Message });
            }
        }
    }
}
