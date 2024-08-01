using CommonInterfaces.DTO;

namespace FGR.Domain.Factories
{
    public interface IWrapperFactory<T>
    {
        IWrapper<T?> Create(T? entity);
        IWrapper<T?> Create(string message);
    }
}
