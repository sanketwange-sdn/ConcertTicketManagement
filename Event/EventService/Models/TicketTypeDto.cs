using System.ComponentModel.DataAnnotations;

namespace EventService.Models
{
    public class AddTicketTypeDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } // e.g., VIP

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }
    public class UpdateTicketTypeDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }

    public class TicketTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } // Total issued quantity
    }
}
