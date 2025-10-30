using Microsoft.AspNetCore.Mvc;
using MyEventsApi.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using MyEventsApi.Dto;

namespace MyEventsApi.Controllers

{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static List<User> _users = new();
        private readonly IConfiguration _config;
        
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_users.Any(u => u.Email == dto.Email))
            {
                return Conflict("User already exists");
            }
            var user = new User
            {
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _users.Add(user);

            return Ok(new { user.Id, user.Email, user.DisplayName });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]LoginDto Dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _users.FirstOrDefault(u => u.Email == Dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(Dto.Password, user.PasswordHash))
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
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
