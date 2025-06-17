using EventService.Entity;
using EventService.EventDb;
using EventService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventDbContext _context;

        public EventController(EventDbContext context)
        {
            _context = context;
        }

        // 📥 Create Event (Admin only)
        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUpdateEvent([FromBody] AddUpdateEventDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 🔄 Update
                if (dto.Id > 0)
                {
                    var ev = await _context.Event.FindAsync(dto.Id);
                    if (ev == null)
                        return NotFound($"Event with ID {dto.Id} not found.");

                    ev.Title = dto.Title;
                    ev.Description = dto.Description;
                    ev.EventDate = dto.EventDate;
                    ev.Venue = dto.Venue;
                    ev.TotalCapacity = dto.TotalCapacity;
                    ev.AvailableTickets = dto.AvailableTickets;

                    await _context.SaveChangesAsync();
                    return Ok("Event updated successfully.");
                }
                else
                {
                    // Add
                    var ev = new Event
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        EventDate = dto.EventDate,
                        Venue = dto.Venue,
                        TotalCapacity = dto.TotalCapacity,
                        AvailableTickets = dto.TotalCapacity
                    };

                    _context.Event.Add(ev);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(null, new { id = ev.Id }, new EventResponseDto
                    {
                        Id = ev.Id,
                        Title = ev.Title,
                        Description = ev.Description,
                        EventDate = ev.EventDate,
                        Venue = ev.Venue,
                        TotalCapacity = ev.TotalCapacity,
                        AvailableTickets = ev.AvailableTickets
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        // 📤 Get All Events (Public)
        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Event
            .Select(ev => new EventResponseDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Venue = ev.Venue,
                TotalCapacity = ev.TotalCapacity,
                AvailableTickets = ev.AvailableTickets
            }).ToListAsync();

            return Ok(events);
        }

        // 🔍 Get Event by ID
        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Event.FindAsync(id);
            if (ev == null) return NotFound();

            return Ok(new EventResponseDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Venue = ev.Venue,
                TotalCapacity = ev.TotalCapacity,
                AvailableTickets = ev.AvailableTickets
            }); 
        }
        // 🔍 Get tikets of event
        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventTicketType(int id)
        {
            var ev = await _context.TicketType.Where(x => x.EventId == id).Select(y => new TicketTypeModel
            {
                Id=y.Id,
                Name=y.Name,
                Price=y.Price,
                Quantity = y.Quantity,
            }).ToListAsync();
            if (ev == null) return NotFound();

            return Ok(ev);
        }
        // 🔍 Get tikets info
        [HttpGet("[action]/{eventId}/{ticketTypeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventTicketType(int eventId, int ticketTypeId)
        {
            var ev = await _context.TicketType.Where(x => x.EventId == eventId && x.Id==ticketTypeId).Select(y => new TicketTypeModel
            {
                Id = y.Id,
                Name = y.Name,
                Price = y.Price,
                Quantity = y.Quantity,
            }).FirstOrDefaultAsync();
            if (ev == null) return NotFound();

            return Ok(ev);
        }

        // Delete Event (Admin only)
        [HttpDelete("[action]/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Event.FindAsync(id);

            if (ev == null)
                return NotFound();

            _context.Event.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
