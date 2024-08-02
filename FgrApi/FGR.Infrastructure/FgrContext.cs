using Microsoft.EntityFrameworkCore;
using FGR.Infrastructure.Models;

namespace FGR.Infrastructure
{
    internal class FgrContext(DbContextOptions<FgrContext> options) : DbContext(options)
    {
        virtual public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
