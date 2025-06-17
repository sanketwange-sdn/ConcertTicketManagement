using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TicketBookingService.Entity;

namespace TicketBookingService.TicketBookingDbContext
{
    public class TicketDbContext : DbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
        }
        public DbSet<Ticket> Ticket { get; set; }
    }
}