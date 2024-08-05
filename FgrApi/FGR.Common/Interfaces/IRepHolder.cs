using System.Security.Principal;

namespace FGR.Common.Interfaces
{
    public interface IRepHolder
    {
        IRepository<I> Repository<I>() where I : class;
        IRepo CustomRepository<IRepo>() where IRepo : class;
        Task Transaction(Func<IRepHolder, Task> func);
        Task SaveAsync(CancellationToken token);
    }
}
