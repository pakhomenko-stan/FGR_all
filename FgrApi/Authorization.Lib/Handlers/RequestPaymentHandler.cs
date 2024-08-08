using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces.Options;

namespace Authorization.Lib.Handlers
{
    public class RequestPaymentHandler(IFgrApiOptions _config) : RequestBaseHandler(_config, handlerScope: FgrTermsHelper.PaymentScope)
    {
    }
}
