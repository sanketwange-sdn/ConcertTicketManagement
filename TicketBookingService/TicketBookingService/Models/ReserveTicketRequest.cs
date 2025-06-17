using System.ComponentModel.DataAnnotations;

namespace TicketBookingService.Models
{
    public class ReserveTicketRequest
    {
        public int EventId { get; set; }
        public int TicketTypeId { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; }
    }
    public class ReserveTicketResponse
    {
        public int TicketId { get; set; }
        public string TicketTypeName { get; set; }
        public decimal PricePerTicket { get; set; }
        public int Quantity { get; set; }
        public decimal Total => PricePerTicket * Quantity;
        public DateTime ExpiresAt { get; set; }
        public string ReservationCode { get; set; }
        public string? Message => "Please Complete your payment to confrim your Ticket, Payment link will expire at "+ ExpiresAt.ToString("MM-dd-yy h mm tt")+ ". Use this "+ ReservationCode + " Reservation Code to process Payment";
    }

}
