using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEventsApi.Models;
using MyEventsApi.Dto;
using MyEventsApi.Data;
using MyEventsApi.Dtos;

namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var item = await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ToListAsync(ct);

            return Ok(item);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var item = await _context.Events
                .AsNoTracking()
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date
            };

            _context.Events.Add(entity);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id, }, entity);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto, CancellationToken ct)
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null) return NotFound();

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Date = dto.Date;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null) return NotFound();

            _context.Events.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("{eventId:int}/join")]
        public async Task<IActionResult> JoinEvent(int eventId, [FromBody] JoinEventRequest req, CancellationToken ct)
        {
            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null) return NotFound($"Event {eventId} not found");

            var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == req.UserId, ct);
            if (!userExists) return NotFound($"User {req.UserId} not found");


            var exists = await _context.Participants
                .AnyAsync(p => p.EventId == eventId && p.UserId == req.UserId, ct);
            if (exists) return Conflict("User already joined this event");


            _context.Participants.Add(new Participant
            {
                EventId = ev.Id,
                UserId = req.UserId,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            return Ok(new { message = "Joined", eventId, req.UserId });
        }


        [HttpPost("{eventId:int}/leave")]
        public async Task<IActionResult> LeaveEvent(int eventId, [FromBody] JoinEventRequest req, CancellationToken ct)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == req.UserId, ct);

            if (participant is null) return NotFound("User is not a participant of this event");

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync(ct);

            return Ok(new { message = "Left", eventId, req.UserId });
        }




        //[HttpGet]
        //public IActionResult GetAll()
        //{

        //    return Ok(_events);
        //}

        //[HttpPost]
        //public  IActionResult Create(EventCreateDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var newEvent = new Event
        //    {
        //        Id = new Random().Next(1000, 9999), // Simulate ID generation
        //        Title = dto.Title,
        //        Description = dto.Description,
        //        Date = dto.Date
        //    };

        //    _events.Add(newEvent);

        //    return Ok(newEvent);

    }
}