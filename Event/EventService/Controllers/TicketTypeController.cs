using EventService.Entity;
using EventService.EventDb;
using EventService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypeController : ControllerBase
    {
        private readonly EventDbContext _context;

        public TicketTypeController(EventDbContext context)
        {
            _context = context;
        }

        // Add Ticket Type
        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTicketType([FromBody] AddTicketTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ev = await _context.Event
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == dto.EventId);

            if (ev == null)
                return NotFound("Event not found.");

            // Check total tickets
            if (ev == null)
                return NotFound("Event not found.");

            // Ensure ev.TicketTypes is not null
            var existingTickets = ev.TicketTypes ?? new List<TicketType>();

            int totalExisting = existingTickets.Sum(t => t.Quantity);
            int totalAfterAdd = totalExisting + dto.Quantity;

            if (totalAfterAdd > ev.TotalCapacity)
            {
                return BadRequest($"Total ticket quantity ({totalAfterAdd}) exceeds event capacity ({ev.TotalCapacity}).");
            }

            var newTicket = new TicketType
            {
                EventId = dto.EventId,
                Name = dto.Name,
                Quantity = dto.Quantity,
                Price = dto.Price
            };

            _context.TicketType.Add(newTicket);
            await _context.SaveChangesAsync();

            return Ok(newTicket);
        }

        // Update Ticket Type by ID
        [HttpPut("[action]/{ticketId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTicketType(int ticketId, [FromBody] UpdateTicketTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _context.TicketType
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound("Ticket type not found.");

            var ev = _context.Event.Find(ticket.EventId);
            if(ev==null)
                return NotFound("Event not found.");
            var AllEventTickets = _context.TicketType.Where(x => x.EventId == ticket.EventId).ToList();

            // Check if new total quantity fits within event capacity
            int totalOtherTickets = 0;
            if (ev !=null && AllEventTickets != null)
            {
                totalOtherTickets = AllEventTickets
                    .Where(t => t.Id != ticketId)
                    .Sum(t => t.Quantity);
            }

            int newTotal = totalOtherTickets + dto.Quantity;

            if (newTotal > ev?.TotalCapacity)
            {
                return BadRequest($"Updated quantity would exceed event capacity ({ev.TotalCapacity}).");
            }

            // Update
            ticket.Name = dto.Name;
            ticket.Quantity = dto.Quantity;
            ticket.Price = dto.Price;

            await _context.SaveChangesAsync();
            return Ok("Ticket type updated successfully.");
        }
    }
}
