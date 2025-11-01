using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using MyEventsApi.Dto;
using MyEventsApi.Data;

namespace MyEventsApi.Controllers

{

    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        
        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email, ct))
                return Conflict("User with this email already exists");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

           

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hash,
                DisplayName = dto.DisplayName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);
            return Ok(new { message = "User registered succesfully", user.Id, });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
            if(user == null) return Unauthorized("Invalid credentials");

            var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if(!ok)
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
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
