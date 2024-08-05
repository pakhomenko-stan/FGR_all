using FGR.Common.Attributes;

namespace FGR.Domain.Interfaces
{
    [RepositoryInterface]
    public interface IUser
    {
        long Id { get; set; }
        string Name { get; set; }
        DateTime CreatedDate { get; set; }
    }
}

