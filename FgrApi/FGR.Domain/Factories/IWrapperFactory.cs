using CommonInterfaces.DTO;

namespace FGR.Domain.Factories
{
    public interface IWrapperFactory
    {
        IWrapper<T?> Create<T>(T? entity);
        IWrapper<T?> Create<T>(string message);
    }
}
