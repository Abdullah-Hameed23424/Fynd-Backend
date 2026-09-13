using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Fynd.Api.Data;
using Fynd.Api.DTOs.Auth;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Fynd.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        private static readonly ConcurrentDictionary<string, OtpData> _otpStore = new();

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        // =========================
        // REGISTER
        // =========================

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                UserName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = string.Empty
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password
            );

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Id = user.Id,
                FullName = user.UserName,
                Email = user.Email,
                Token = token
            };
        }


        // =========================
        // LOGIN
        // =========================

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Id = user.Id,
                FullName = user.UserName,
                Email = user.Email,
                Token = token
            };
        }


        // =========================
        // FORGOT PASSWORD
        // =========================

        public async Task<string?> ForgotPasswordAsync(
            ForgotPasswordRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
                return null;

            var otp = GenerateOtp();

            _otpStore[email] = new OtpData
            {
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            return otp;
        }


        // =========================
        // VERIFY OTP
        // =========================

        public Task<string?> VerifyOtpAsync(
            VerifyOtpRequest request)
        {
            var email = request.Email.Trim().ToLower();

            if (!_otpStore.TryGetValue(email, out var otpData))
                return Task.FromResult<string?>(null);

            if (otpData.ExpiresAt < DateTime.UtcNow)
            {
                _otpStore.TryRemove(email, out _);
                return Task.FromResult<string?>(null);
            }

            if (otpData.Otp != request.Otp)
                return Task.FromResult<string?>(null);

            _otpStore.TryRemove(email, out _);

            var resetToken = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32)
            );

            _otpStore[email] = new OtpData
            {
                Otp = resetToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsResetToken = true
            };

            return Task.FromResult<string?>(resetToken);
        }


        // =========================
        // RESET PASSWORD
        // =========================

        public async Task<bool> ResetPasswordAsync(
            ResetPasswordRequest request)
        {
            var record = _otpStore
                .FirstOrDefault(x =>
                    x.Value.Otp == request.ResetToken &&
                    x.Value.IsResetToken);

            if (record.Key == null)
                return false;

            if (record.Value.ExpiresAt < DateTime.UtcNow)
            {
                _otpStore.TryRemove(record.Key, out _);
                return false;
            }

            var email = record.Key;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                _otpStore.TryRemove(email, out _);
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.NewPassword
            );

            await _context.SaveChangesAsync();

            _otpStore.TryRemove(email, out _);

            return true;
        }


        // =========================
        // JWT
        // =========================

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException(
                    "JWT Key is not configured."
                );

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        // =========================
        // OTP GENERATOR
        // =========================

        private static string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(0, 1000000)
                .ToString("D6");
        
        }


        // =========================
        // OTP DATA
        // =========================

        private class OtpData
        {
            public string Otp { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }

            public bool IsResetToken { get; set; }
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }
    }
}