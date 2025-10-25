using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HospitalManagementSystemAPI.Data;
using HospitalManagementSystemAPI.Helpers;
using HospitalManagementSystemAPI.Models;
using HospitalManagementSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace HospitalManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwt;
        private readonly IEmailService _emailService;

        public AdminsController(AppDbContext context, IOptions<JwtSettings> jwtSettings, IEmailService emailService)
        {
            _context = context;
            _jwt = jwtSettings.Value;
            _emailService = emailService;
        }

        // POST: api/admins/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password are required.");

            if (await _context.Admins.AnyAsync(a => a.Email == req.Email))
                return BadRequest("Email already registered.");

            var (hash, salt) = HashPassword(req.Password);
            var admin = new Admin
            {
                Email = req.Email,
                PasswordHash = $"{Convert.ToBase64String(salt)}:{hash}" // store salt:hash
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            // Send welcome email safely
            try
            {
                var html = $"<p>Hi,</p><p>You are registered to Hospital Management System (HMS) with email {req.Email}.</p>";
                await _emailService.SendEmailAsync(req.Email, "HMS Registration", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            return Ok(new { message = "Registered successfully. Please login." });
        }

        // POST: api/admins/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password are required.");

            // Use AsNoTracking to prevent EF overhead
            var admin = await _context.Admins
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Email == req.Email);

            if (admin == null) return Unauthorized("Invalid credentials");

            if (!VerifyPassword(req.Password, admin.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(admin);
            return Ok(new { token, email = admin.Email });
        }

        // POST: api/admins/forgot
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotRequest req)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == req.Email);
            if (admin == null) return Ok(new { message = "If the email exists, a reset link was sent." });

            // generate token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            admin.ResetToken = token;
            admin.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = $"{req.ClientResetUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(admin.Email)}";

            try
            {
                var html = $"<p>You requested a password reset. Click the link to reset:</p><p><a href=\"{resetLink}\">Reset Password</a></p>";
                await _emailService.SendEmailAsync(admin.Email, "HMS Password Reset", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Password reset email failed: " + ex.Message);
            }

            return Ok(new { message = "If the email exists, a reset link was sent." });
        }

        // POST: api/admins/reset
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetRequest req)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == req.Email && a.ResetToken == req.Token);
            if (admin == null) return BadRequest("Invalid token or email.");

            if (!admin.ResetTokenExpiry.HasValue || admin.ResetTokenExpiry.Value < DateTime.UtcNow)
                return BadRequest("Token expired.");

            var (hash, salt) = HashPassword(req.NewPassword);
            admin.PasswordHash = $"{Convert.ToBase64String(salt)}:{hash}";
            admin.ResetToken = null;
            admin.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successful. Please login." });
        }

        // Protected endpoint to test authentication
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(new { email });
        }

        // ----- helpers -----
        private string GenerateJwtToken(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Key);
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new Claim(ClaimTypes.Email, admin.Email)
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static (string hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 256 / 8));
            return (hashed, salt);
        }

        private static bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = parts[1];
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 256 / 8));
            return hashed == storedHash;
        }

        // DTOs
        public class RegisterRequest { public string Email { get; set; } = null!; public string Password { get; set; } = null!; }
        public class LoginRequest { public string Email { get; set; } = null!; public string Password { get; set; } = null!; }
        public class ForgotRequest { public string Email { get; set; } = null!; public string ClientResetUrl { get; set; } = null!; }
        public class ResetRequest { public string Email { get; set; } = null!; public string Token { get; set; } = null!; public string NewPassword { get; set; } = null!; }
    }
}
