using System.ComponentModel.DataAnnotations;

namespace TicketBookingService.Models
{
    public class ConfirmTicketRequest
    {
        [Required]
        public string ReservationCode { get; set; }
    }
}
