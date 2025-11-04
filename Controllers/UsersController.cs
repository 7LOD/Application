using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyEventsApi.Data;
using MyEventsApi.Models;
using MyEventsApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me/events")]
        [Authorize]
        public async Task<IActionResult> GetMyEvents(CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            var events = await _userService.GetUserEventsAsync(userId, ct);
            return Ok(events);
        }

        private Guid GetUserIdFromToken()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.Parse(sub!);
        }
    }
}
