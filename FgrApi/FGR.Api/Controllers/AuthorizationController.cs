using System.Security.Claims;
using Authorization.Lib.Filters;
using Authorization.Lib.Helpers;
using FGR.Common.Interfaces;
using FGR.Domain.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FGR.Api.Controllers
{
    public class AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IRepHolder repHolder
        ) : Controller
    {
        /// <summary>
        /// Supply access token as reply to request with username/password or to request with refresh token
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpPost("~/" + FgrTermsHelper.tokenRoute), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsClientCredentialsGrantType()) return await SignInServerClient(request);

            return ForbidClause(Errors.InvalidGrant, "Invalid request");
        }

        #region Private methods
        private static IEnumerable<string> FullClaimsDestinations
        {
            get =>
            [
                Destinations.AccessToken,
                Destinations.IdentityToken
            ];
        }

        private static IEnumerable<string> GetDestinations(Claim claim) => 
            claim.Type switch
            {
                "secret_value" => [],
                _ => [Destinations.AccessToken]
            };

        private static void SetClaimDestinations(ClaimsPrincipal principal) =>
            principal.Claims.ToList().ForEach(claim => claim.SetDestinations(GetDestinations(claim)));


        private async Task<IActionResult> SignInServerClient(OpenIddictRequest request)
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var application = await applicationManager.FindByClientIdAsync(request?.ClientId ?? string.Empty)
                ?? throw new InvalidOperationException("The application details cannot be found in the database.");

            var scope = request?.Scope;
            var clientId = request?.ClientId;

            string[] scopes = [FgrTermsHelper.AdminUIScope, FgrTermsHelper.PaymentScope];
            if (string.IsNullOrEmpty(scope) || !scopes.Contains(scope)) return ForbidClause(Errors.InvalidGrant, "Invalid request");

            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
            identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));
            identity.AddClaim(Claims.Scope, scope);
            AppendClaims(identity, scope, clientId);

            var principal = new ClaimsPrincipal(identity);
            SetClaimDestinations(principal);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private void AppendClaims(ClaimsIdentity identity, string scope, string? clientId = null)
        {
            switch (scope)
            {
                case FgrTermsHelper.AdminUIScope:
                    {
                        return;
                    }
                case FgrTermsHelper.PaymentScope:
                    {
                        var id = long.TryParse(clientId ?? string.Empty, out long result) ? result : 0;
                        var entry = id > 0 ? repHolder.Repository<IAuthClientDataResult>().GetByID(id) : null;
                        if (entry != null)
                        {
                            identity.SetClaim(FgrTermsHelper.ProjectIdClaimType, entry.CompanyProjectId.HasValue ? entry.CompanyProjectId.ToString() : string.Empty);
                            identity.SetClaim(FgrTermsHelper.CompanyIdClaimType, entry.CompanyId.ToString());
                            identity.SetClaim(FgrTermsHelper.TypeClaimType, entry.CompanyProjectId.HasValue ? FgrTermsHelper.ProjectType : FgrTermsHelper.CompanyType);

                            identity.SetClaim(FgrTermsHelper.TransactionKeyIdClaimType, $"{entry.TransactionKeyId}");
                            identity.SetClaim(FgrTermsHelper.TransactionPublicKeyClaimType, entry.TransactionPublicKey ?? string.Empty);
                        }
                        return;
                    }
                default: return;
            }
        }

        private ForbidResult ForbidClause(string err, string description) => Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = err,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            })
        );

        #endregion

    }
}
