using Microsoft.EntityFrameworkCore;
using Solution.Core.Entity;
namespace Solution.Persistence
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }

    }
}
