namespace TicketBookingService.Models
{
    public class TicketTypeFromEventServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } // Total issued quantity
    }
}
