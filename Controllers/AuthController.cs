using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using MyEventsApi.Dtos;
using MyEventsApi.Data;
using MyEventsApi.Services.Interfaces;

namespace MyEventsApi.Controllers

{

    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var result = await _authService.RegisterAsync(dto, ct);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {            
            var result = await _authService.LoginAsync(dto, ct);
            if (result is null)
            {
                return Unauthorized("Invalid email or password.");
            }
            return Ok(result);
        }


        
        
    }
}
