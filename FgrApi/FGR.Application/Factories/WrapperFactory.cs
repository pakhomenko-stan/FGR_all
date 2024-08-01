using CommonInterfaces.DTO;
using FGR.Domain.Factories;

namespace FGR.Application.Factories
{
    public class WrapperFactory<T> : IWrapperFactory<T> where T : class 
    {
        private class Wrapper : IWrapper<T?>
        {
            public T? Data { get; set; }
            public string? Title { get; set; }
            public DataTransferStatus Status { get; set; }
            public string Message { get; set; } = null!;
        }

        public IWrapper<T?> Create(T? entity)
        {
            var result = new Wrapper
            {
                Data = entity,
                Title = "Wrapper",
                Status = DataTransferStatus.Success,
                Message = string.Empty,
            };
            return result;
        }

        public IWrapper<T?> Create(string message)
        {
            var result = new Wrapper
            {
                Data = null,
                Title = "Wrapper",
                Status = DataTransferStatus.Failure,
                Message = message,
            };
            return result;
        }
    }

}
