using CommonInterfaces.DTO;
using CommonInterfaces.Services;
using FGR.Domain.Factories;
using Microsoft.AspNetCore.Mvc;

namespace FGR.Api.Controllers
{
    public class BaseFgrController : ControllerBase
    {
        protected async Task<IWrapper<TOut?>> ExecutorWrapper<TOut>(Func<Task<TOut?>> function)
        {
            var scope = ControllerContext.HttpContext.RequestServices.CreateScope();
            var sp = scope.ServiceProvider;
            IWrapperFactory factory = sp.GetRequiredService<IWrapperFactory>() ?? throw new Exception(); //TODO - specify exception
            try
            {
                var data = await function.Invoke();
                IWrapper<TOut?> result = factory.Create(data);
                return result;
            }
            catch (Exception e)
            {
                IWrapper<TOut?> reply = factory.Create<TOut>(e.Message);
                return reply;

            }
        }
        protected async Task<ActionResult<IWrapper<TOut?>>> ActionAsync<TSrv, TOut>(TSrv action)
            where TOut : class
            where TSrv : class, IExecutor<TOut>
        {
            var result = await ExecutorWrapper(async () => await action.ExecuteAsync());
            return Ok(result);
        }
        protected async Task<ActionResult<IWrapper<TOut?>>> ActionWithParameterAsync<TPar, TSrv, TOut>(TPar param, TSrv action)
            where TOut : class
            where TSrv : class, IExecutor<TOut>
        {
            var result = await ExecutorWrapper(async () => await action.ExecuteAsync(param));
            return Ok(result);
        }

        protected async Task<ActionResult<IWrapper<TOut?>>> ActionAsync<TIn, TSrv, TOut>(TSrv action, TIn request)
            where TOut : class
            where TIn : class
            where TSrv : class, IExecutor<TIn, TOut>
        {
            var result = await ExecutorWrapper(async () => await action.ExecuteAsync(request));
            return Ok(result);
        }

        protected async Task<ActionResult<IWrapper<TOut>>> ActionWithParameterAsync<TPar, TIn, TSrv, TOut>(TPar param, TSrv action, TIn? request = default)
            where TOut : class
            where TIn : class
            where TSrv : class, IExecutor<TIn, TOut>
        {
            var result = await ExecutorWrapper(async () => await action.ExecuteAsync(param, request));
            return Ok(result);
        }


    }
}
