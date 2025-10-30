using Microsoft.AspNetCore.Mvc;
using MyEventsApi.Models;
using MyEventsApi.Dto;
namespace MyEventsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private static readonly List<Event> _events = new()
        {
            new Event { Id = 1, Title = "First Event", Description = "Test event", Date = DateTime.UtcNow.AddDays(1) },
            new Event { Id = 2, Title = "Second Event", Description = "Another event", Date = DateTime.UtcNow.AddDays(2) },
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            
            return Ok(_events);
        }

        [HttpPost]
        public IActionResult Create(EventCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newEvent = new Event
            {
                Id = new Random().Next(1000, 9999), // Simulate ID generation
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date
            };

            _events.Add(newEvent);

            return Ok(newEvent);

        }
    }

}