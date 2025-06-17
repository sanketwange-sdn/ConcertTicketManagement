using EventService.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EventService.EventDb
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
        {
        }
        public DbSet<Event> Event { get; set; }
        public DbSet<TicketType> TicketType { get; set; }

    }
}
