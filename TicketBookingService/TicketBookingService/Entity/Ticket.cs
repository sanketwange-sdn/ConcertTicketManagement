using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketBookingService.Common;

namespace TicketBookingService.Entity
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Extracted from JWT

        [Required]
        public int TicketTypeId { get; set; } // Comes from EventService

        public string TicketTypeName { get; set; } = string.Empty; // Optional: for display

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal PricePerTicket { get; set; } // Save at time of reservation

        public decimal TotalPrice => PricePerTicket * Quantity;

        public TicketStatusEnum  Status { get; set; }

        public DateTime ReservedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? PurchasedAt { get; set; }

        public string? ReservationCode { get; set; } // Optional for tracking
    }
}
