namespace Authorization.SSO.Filters
{
    /// <summary>
    /// Request to this controller may be originated only from allowed IP addresses (ServerOptions.AllowedIPAddresses) 
    /// </summary>
    internal class RequestAddressFilter : ActionFilterAttribute
    {
        private readonly List<string> ipList;

        public RequestAddressFilter(ServerOptions options)
        {
            ipList = (options?.AllowedIPAddresses?.Count() ?? 0) > 0 ? options.AllowedIPAddresses.ToList() : null;
        }

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
            var islocal = ipList?.Contains(context.HttpContext.Connection.RemoteIpAddress.ToString()) ?? true;
            var claimId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == AuthParams.AuthParams.idClaim);
            var isServer = claimId.Value == "server";
            return islocal && isServer;
        }

        private static void SetResponseOfForbidden(ActionExecutingContext context)
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }

    }
}
