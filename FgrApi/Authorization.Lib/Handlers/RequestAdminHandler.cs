using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces.Options;

namespace Authorization.Lib.Handlers
{
    public class RequestAdminHandler(IFgrApiOptions _config) : RequestBaseHandler(_config, handlerScope: FgrTermsHelper.AdminUIScope)
    {
    }
}
