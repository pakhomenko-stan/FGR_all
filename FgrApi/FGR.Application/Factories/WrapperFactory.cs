using CommonInterfaces.DTO;
using FGR.Common.Attributes;
using FGR.Domain.Factories;

namespace FGR.Application.Factories
{
    [Implementable(ImplementableType.Factory)]
    public class WrapperFactory : IWrapperFactory
    {
        private class Wrapper<T> : IWrapper<T>
        {
            public T? Data { get; set; }
            public string? Title { get; set; }
            public DataTransferStatus Status { get; set; }
            public string Message { get; set; } = null!;
        }

        public IWrapper<T?> Create<T>(T? entity)
        {
            var result = new Wrapper<T?>
            {
                Data = entity,
                Title = "Wrapper",
                Status = DataTransferStatus.Success,
                Message = string.Empty,
            };
            return result;
        }

        public IWrapper<T?> Create<T>(string message)
        {
            var result = new Wrapper<T?>
            {
                Data = default,
                Title = "Wrapper",
                Status = DataTransferStatus.Failure,
                Message = message,
            };
            return result;
        }
    }

}
