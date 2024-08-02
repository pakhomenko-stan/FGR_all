using FGR.Domain.Interfaces;

namespace FGR.Infrastructure.Models
{
    public class User : IUser
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
