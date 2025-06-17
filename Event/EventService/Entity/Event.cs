using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace EventService.Entity
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EventDate { get; set; }

        public string Venue { get; set; }

        public int TotalCapacity { get; set; }

        public int AvailableTickets { get; set; }

        // property: One Event → Many TicketTypes
        public ICollection<TicketType>? TicketTypes { get; set; }
    }
}
