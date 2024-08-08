using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces;

namespace Authorization.Lib.Handlers
{
    public class RequestPaymentHandler(IFgrApiConfig _config) : RequestBaseHandler(_config, handlerScope: FgrTermsHelper.PaymentScope)
    {
    }
}
