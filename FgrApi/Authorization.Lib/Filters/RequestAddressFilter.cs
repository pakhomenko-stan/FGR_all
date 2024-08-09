using System.Net;
using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Authorization.Lib.Filters
{
    /// <summary>
    /// Request to this controller may be originated only from allowed IP addresses (ServerOptions.AllowedIPAddresses) 
    /// </summary>
    public class RequestAddressFilter(IFgrApiOptions options) : ActionFilterAttribute
    {
        private readonly List<string> ipList = options?.AllowedIPAddresses?.ToList() ?? [];

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsAllowed(context))
            {
                SetResponseOfForbidden(context);
                return;
            }
            base.OnActionExecuting(context);
        }

        private bool IsAllowed(ActionExecutingContext context)
        {
            var ip = context?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
            var islocal = ipList?.Contains(ip) ?? true;
            return islocal;
        }

        private static void SetResponseOfForbidden(ActionExecutingContext context)
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }

    }
}
