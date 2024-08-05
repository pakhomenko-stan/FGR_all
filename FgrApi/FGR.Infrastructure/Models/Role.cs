using FGR.Domain.Interfaces;

namespace FGR.Infrastructure.Models
{
    public class Role : IRole
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
    }
}
