using FGR.Domain.Interfaces;

namespace FGR.Infrastructure.Models
{
    internal class User : IUser
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
