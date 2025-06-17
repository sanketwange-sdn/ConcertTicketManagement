using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventService.Entity
{
    public class TicketType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., VIP, General

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Foreign key to Event
        [ForeignKey("Event")]
        public int EventId { get; set; }

        public Event? Event { get; set; }
    }
}
