using Microsoft.EntityFrameworkCore;

namespace FGR.Infrastructure
{
    internal class FgrContext(DbContextOptions<FgrContext> options) : DbContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
