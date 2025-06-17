using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using TicketBookingService.Abstract;
using TicketBookingService.Common;
using TicketBookingService.Entity;
using TicketBookingService.Models;
using TicketBookingService.TicketBookingDbContext;

namespace TicketBookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketBookingController : ControllerBase
    {
        private readonly TicketDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public TicketBookingController(TicketDbContext context, ICurrentUserService currentUserService, IHttpClientFactory httpClientFactory, IConfiguration config, IRabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _currentUserService = currentUserService;
            _httpClientFactory = httpClientFactory;
            _config = config;
           _rabbitMqPublisher= rabbitMqPublisher; ;
        }


        [HttpGet("[action]/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTickets(int eventId)
        {
            // 1. Call EventService to get ticket types
            var client = _httpClientFactory.CreateClient();
            var eventServiceBaseUrl = _config["EventServiceBaseUrl"]; // e.g. https://localhost:44330

            var response = await client.GetAsync($"{eventServiceBaseUrl}/api/Event/GetEventTicketType/{eventId}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Unable to fetch ticket types from EventService.");

            var ticketTypes = await response.Content.ReadFromJsonAsync<List<TicketTypeFromEventServiceModel>>();

            var availability = new List<AvailableTicketDto>();

            foreach (var ticketType in ticketTypes)
            {
                var used = await _context.Ticket
                    .Where(t => t.TicketTypeId == ticketType.Id &&
                               (t.Status == TicketStatusEnum.Purchased ||
                               (t.Status == TicketStatusEnum.Reserved && t.ExpiresAt > DateTime.UtcNow)))
                    .SumAsync(t => t.Quantity);

                availability.Add(new AvailableTicketDto
                {
                    TicketTypeId = ticketType.Id,
                    Name = ticketType.Name,
                    Price = ticketType.Price,
                    OriginalQuantity = ticketType.Quantity,
                    Remaining = Math.Max(0, ticketType.Quantity - used)
                });
            }

            return Ok(availability);
        }

        [HttpPost("reserve")]
        [Authorize]
        public async Task<IActionResult> ReserveTicket([FromBody] ReserveTicketRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Get TicketType info from EventService
            var client = _httpClientFactory.CreateClient();
            var eventServiceUrl = _config["EventServiceBaseUrl"];
            var ticketTypeResp = await client.GetAsync($"{eventServiceUrl}/api/Event/GetEventTicketType/{request.EventId}/{request.TicketTypeId}");

            if (!ticketTypeResp.IsSuccessStatusCode)
                return BadRequest("Invalid TicketTypeId.");

            var ticketType = await ticketTypeResp.Content.ReadFromJsonAsync<TicketTypeFromEventServiceModel>();
            if (ticketType == null)
                return BadRequest("Ticket type not found.");

            // 2. Check availability
            var used = await _context.Ticket
                .Where(t => t.TicketTypeId == request.TicketTypeId &&
                           (t.Status == TicketStatusEnum.Purchased ||
                            (t.Status == TicketStatusEnum.Reserved && t.ExpiresAt > DateTime.Now)))
                .SumAsync(t => t.Quantity);

            var remaining = ticketType.Quantity - used;
            if (request.Quantity > remaining)
                return BadRequest($"Only {remaining} tickets left for this type.");

            // 3. Create ticket record
            var GuId = Guid.NewGuid().ToString("N");
            var ticket = new Ticket
            {
                UserId = _currentUserService.UserId,
                TicketTypeId = ticketType.Id,
                TicketTypeName = ticketType.Name,
                Quantity = request.Quantity,
                PricePerTicket = ticketType.Price,
                Status = TicketStatusEnum.Reserved,
                ReservedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                ReservationCode = GuId
            };

            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new ReserveTicketResponse
            {
                TicketId = ticket.Id,
                TicketTypeName = ticket.TicketTypeName,
                PricePerTicket = ticket.PricePerTicket,
                Quantity = ticket.Quantity,
                ExpiresAt = ticket.ExpiresAt,
                ReservationCode = GuId
            });
        }

        [HttpPost("confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmTicket([FromBody] ConfirmTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ReservationCode))
                return BadRequest("Reservation code is required.");

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(t =>
                    t.ReservationCode == request.ReservationCode &&
                    t.UserId == _currentUserService.UserId);

            if (ticket == null)
                return NotFound("Ticket not found for this user.");

            if (ticket.Status != TicketStatusEnum.Reserved)
                return BadRequest("Only reserved tickets can be confirmed.");

            if (ticket.ExpiresAt <= DateTime.Now)
            {
                ticket.Status = TicketStatusEnum.Cancelled;
                await _context.SaveChangesAsync();
                return BadRequest("Reservation has expired and is now cancelled.");
            }

            ticket.Status = TicketStatusEnum.Purchased;
            ticket.PurchasedAt = DateTime.Now;

            var emailMessage = new EmailMessageModel
            {
                To = _currentUserService.Email, // or hardcoded for testing
                Subject = "Ticket Confirmation",
                Body = $"Hi {_currentUserService.FullName},\n\nThanks for booking {ticket.Quantity}x {ticket.TicketTypeName}.\nTicket ID: {ticket.Id}"
            };
            _rabbitMqPublisher.Publish(emailMessage, "emailQueue");

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket successfully purchased.",
                ticketType = ticket.TicketTypeName,
                quantity = ticket.Quantity,
                totalPrice = ticket.Quantity * ticket.PricePerTicket,
                purchasedAt = ticket.PurchasedAt
            });
        }
    }
}
