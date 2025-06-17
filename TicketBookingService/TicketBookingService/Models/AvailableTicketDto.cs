namespace TicketBookingService.Models
{
    public class AvailableTicketDto
    {
        public int TicketTypeId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int OriginalQuantity { get; set; }
        public int Remaining { get; set; }
    }
}
