using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Data;
using MyEventsApi.Dtos;

using MyEventsApi.Models;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MyEventsApi.Utils;
using MyEventsApi.Services.Interfaces;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var events = await _eventService.GetAllAsync(ct);
            return Ok(events);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var ev = await _eventService.GetByIdAsync(id, ct);
            return ev is null ? NotFound() : Ok(ev);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto, CancellationToken ct)
        {
            var result = await _eventService.CreateEventAsync(dto, GetUserIdFromToken(), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);

        }

    
        [HttpPatch("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto, CancellationToken ct)
        {
            var success = await _eventService.UpdateEventAsync(id, dto, GetUserIdFromToken(), ct);
            return success ? NoContent() : Forbid("You are not the organizer or event not found");
        }


        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var success = await _eventService.DeleteEventAsync(id, GetUserIdFromToken(), ct);
            return success ? NoContent() : Forbid("You are not the organizer or event not found");
        }

        private Guid GetUserIdFromToken()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.Parse(sub!);
        }


        [HttpPost("{eventId:int}/join")]
        [Authorize]
        public async Task<IActionResult> JoinEvent(int eventId, CancellationToken ct)
        {
            var success = await _eventService.JoinEventAsync(eventId, GetUserIdFromToken(), ct);
            return success ? Ok(new { message = "Joined the event successfully." }) : BadRequest("Could not join the event. It may be full or you have already joined.");
        }


        [HttpPost("{eventId:int}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveEvent(int eventId, CancellationToken ct)
        {
            var success = await _eventService.LeaveEventAsync(eventId, GetUserIdFromToken(), ct);
            return success ? Ok(new { message = "Left the event successfully." }) : BadRequest("Could not leave the event. You may not be a participant.");
        }
        

        [HttpGet("calendar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventsInPeriod([FromQuery] DateTime start, [FromQuery] DateTime end, CancellationToken ct)
        {
            if (end <= start)
                return BadRequest("End date must be after start date.");
            var items = await _eventService.GetEventInPeriodAsync(start, end, ct);
            return Ok(items);


        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchEvents([FromQuery] string query, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            var items = await _eventService.SearchEventAsync(query, ct);
            return Ok(items);


        }
    }
}