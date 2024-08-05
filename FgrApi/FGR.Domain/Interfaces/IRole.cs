using FGR.Common.Attributes;

namespace FGR.Domain.Interfaces
{
    [RepositoryInterface]

    public interface IRole
    {
        long Id { get; set; }
        string Name { get; set; }
    }
}
