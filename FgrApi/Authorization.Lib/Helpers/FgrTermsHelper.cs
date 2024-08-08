namespace Authorization.Lib.Helpers
{
    public class FgrTermsHelper
    {
        private const string routePrefix = "connect";
        public const string tokenApiRoute = routePrefix + "/api/token";
        public const string tokenRoute = routePrefix + "/token";
        public const string autorizeRoute = routePrefix + "/authorize";
        public const string logoutRoute = routePrefix + "/logout";
        public const string userInfoRoute = routePrefix + "/userinfo";
        public const string idClaim = "sub";


        public const string AdminUIScope = "AdminUIScope";
        public const string PaymentScope = "PaymentScope";

        public const string AdminUIAuthenticationScheme = "AdminUIScheme";
        public const string AdminUIPolicy = "AdminUIPolicy";

        public const string PaymentAuthenticationScheme = "PaymentScheme";
        public const string PaymentPolicy = "PaymentPolicy";

        public const string CompanyType = "Company";
        public const string ProjectType = "Project";

        public const string WebClientId = "WebClientId";
        public const string WebClientDisplayName = "WebClient";

        public const string ProjectIdClaimType = "projectIdClaim";
        public const string TypeClaimType = "typeClaim";
        public const string CompanyIdClaimType = "companyIdClaim";
        public const string TransactionPublicKeyClaimType = "transactionPublicKeyClaim";
        public const string TransactionKeyIdClaimType = "transactionKeyIdClaim";

        public static string GetClientClaim(string claimType)
        {
            return $"client_{claimType}";
        }

    }
}
