using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyEventsApi.Data;
using MyEventsApi.Dtos;
using MyEventsApi.Models;
using MyEventsApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyEventsApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email, ct))
                throw new InvalidOperationException("User with this email already exists");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hash,
                DisplayName = dto.DisplayName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
            if (user is null) return null;

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValid) return null;

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            };  



        } 
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.DisplayName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
