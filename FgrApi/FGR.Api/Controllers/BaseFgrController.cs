using System.Security.Claims;
using CommonInterfaces.DTO;
using CommonInterfaces.Services;
using FGR.Domain.Factories;
using Microsoft.AspNetCore.Mvc;

namespace FGR.Api.Controllers
{
    public class BaseFgrController : ControllerBase
    {
        protected async Task<IWrapper<TOut?>> ExecutorWrapper<TOut>(Func<ClaimsPrincipal, Task<TOut?>> function)
        {
            var principal = ControllerContext.HttpContext.User;

            IWrapperFactory factory = ControllerContext.HttpContext.RequestServices.GetRequiredService<IWrapperFactory>() ?? throw new Exception(); //TODO - specify exception
            try
            {
                var data = await function.Invoke(principal);
                IWrapper<TOut?> result = factory.Create(data);
                return result;
            }
            catch (Exception e)
            {
                IWrapper<TOut?> reply = factory.Create<TOut>(e.Message);
                return reply;

            }
        }
        protected async Task<ActionResult<IWrapper<TOut?>>> ActionAsync<TSrv, TOut>(TSrv action, CancellationToken token)
            where TOut : class
            where TSrv : class, IExecutor<TOut>
        {
            var result = await ExecutorWrapper(async principal => await action.ExecuteAsync(principal, token));
            return Ok(result);
        }
        protected async Task<ActionResult<IWrapper<TOut?>>> ActionWithParameterAsync<TPar, TSrv, TOut>(TPar param, TSrv action, CancellationToken token)
            where TOut : class
            where TSrv : class, IExecutor<TOut>
        {
            var result = await ExecutorWrapper(async principal => await action.ExecuteAsync(param, principal, token));
            return Ok(result);
        }

        protected async Task<ActionResult<IWrapper<TOut?>>> ActionAsync<TIn, TSrv, TOut>(TSrv action, TIn request, CancellationToken token)
            where TOut : class
            where TIn : class
            where TSrv : class, IExecutor<TIn, TOut>
        {
            var result = await ExecutorWrapper(async principal => await action.ExecuteAsync(request, principal, token));
            return Ok(result);
        }

        protected async Task<ActionResult<IWrapper<TOut>>> ActionWithParameterAsync<TPar, TIn, TSrv, TOut>(TPar param, TSrv action, TIn? request, CancellationToken token)
            where TOut : class
            where TIn : class
            where TSrv : class, IExecutor<TIn, TOut>
        {
            var result = await ExecutorWrapper(async principal => await action.ExecuteAsync(param, request, principal, token));
            return Ok(result);
        }


    }
}
