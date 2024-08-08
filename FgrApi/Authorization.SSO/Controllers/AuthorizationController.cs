using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization.Core.Models;
using Authorization.Lib.Helpers;
using Authorization.SSO.Attributes;
using Authorization.SSO.ViewModels;
using Authorization.SSO.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Authorization.Lib.Interfaces.Options;

namespace Authorization.SSO.Controllers
{
    public class AuthorizationController(IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        IFgrApiOptions config,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<AuthorizationController> logger) : Controller
    {

        private readonly IOpenIddictApplicationManager _applicationManager = applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager = authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager = scopeManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AuthorizationController> _logger = logger;

        /// <summary>
        /// Main authorization
        /// </summary>
        [HttpGet("~/" + FgrTermsHelper.autorizeRoute)]
        [HttpPost("~/" + FgrTermsHelper.autorizeRoute)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.HasPrompt(Prompts.Login))
                return LoginRequestRedirect(request);

            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (!IsValidAndNotExpired(result, request))
                return InvalidAuthorizeRequestRedirect(request);

            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            if (user.IsInactive)
                return InvalidAuthorizeRequestRedirect(request);

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var authorizations = await UserAuthorizationAsync(request, user, application);
            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case ConsentTypes.External when !authorizations.Any():
                    return ForbidClause(Errors.ConsentRequired, "User access to this application is forbidden");

                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                    return await PrincipalSignInAsync(user, request, application, authorizations);

                case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                    return ForbidClause(Errors.ConsentRequired, "Interactive user consent is required.");

                default:
                    return View(new AuthorizeViewModel
                    {
                        ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                        Scope = request.Scope
                    });
            }
        }

        /// <summary>
        /// Accepted authorization as result of consent request (user allows for application use auth data)
        /// </summary>
        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/" + FgrTermsHelper.autorizeRoute), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var authorizations = await UserAuthorizationAsync(request, user, application);

            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
                return ForbidClause(Errors.ConsentRequired, "Access is forbidden for this user");

            return await PrincipalSignInAsync(user, request, application, authorizations);
        }

        /// <summary>
        /// Denied authorization as result of consent request (user prohibits for application use auth data)
        /// </summary>
        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/" + FgrTermsHelper.autorizeRoute), ValidateAntiForgeryToken]
        public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        /// <summary>
        /// Get logout with redirect to logout page
        /// </summary>
        /// <returns></returns>
        [HttpGet("~/" + FgrTermsHelper.logoutRoute)]
        public async Task<IActionResult> Logout() => await LogoutPost();

        /// <summary> 
        /// Returning a SignOutResult will ask OpenIddict to redirect the user agent 
        /// to the post_logout_redirect_uri specified by the client application or to 
        /// the RedirectUri specified in the authentication properties if none was set.
        /// </summary>
        [ActionName(nameof(Logout)), HttpPost("~/" + FgrTermsHelper.logoutRoute), ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties { RedirectUri = "/" });
        }

        [HttpGet("~/" + FgrTermsHelper.tokenApiRoute)]
        [HttpPost("~/" + FgrTermsHelper.tokenApiRoute)]
        public IActionResult ExchangeApi(string username, string password)
        {
            var client = new System.Net.Http.HttpClient();
            var request = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password
            });

            var uri = Url.Action(action: "Exchange", controller: "Authorization", values: null, protocol: Request.Scheme);

            _logger.LogInformation("Forwarding token request to {Uri}", uri);

            var reply = client.PostAsync(uri, request).GetAwaiter().GetResult();
            var replyJson = reply.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            _logger.LogInformation("Forwared token request response: {ReplyJson}", replyJson);

            var replyObject = JsonConvert.DeserializeObject<JToken>(replyJson);

            if (replyObject["error"] != null && !string.IsNullOrEmpty(replyObject["error"].ToString()))
                throw new Exception(replyObject["error_description"].ToString());

            var token = replyObject["access_token"].Value<string>();
            return Ok(new { access_token = token });
        }

        /// <summary>
        /// Supply access token as reply to request with username/password or to request with refresh token
        /// </summary>
        /// <returns></returns>
        [HttpPost("~/" + FgrTermsHelper.tokenRoute), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsPasswordGrantType()) return await SignInResultByPassword(request);
            if (request.IsClientCredentialsGrantType()) return await SignInServerClient(request);

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
                throw new NotImplementedException("The specified grant is not supported.");

            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            var user = await _userManager.GetUserAsync(principal);

            if (user == null) return ForbidClause(Errors.InvalidGrant, "Invalid token");
            if (!await _signInManager.CanSignInAsync(user)) return ForbidClause(Errors.InvalidGrant, "User access is denied");

            SetClaimDestinations(principal);

            return SignIn(
                new ClaimsPrincipal(principal),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/" + FgrTermsHelper.userInfoRoute)]
        public async Task<IActionResult> Userinfo()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            return Ok(user);
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

        private async Task<IActionResult> SignInServerClient(OpenIddictRequest request)
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("The application details cannot be found in the database.");

            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
            identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));


            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        private async Task<IActionResult> SignInResultByPassword(OpenIddictRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return ForbidClause(Errors.InvalidGrant, "The username/password couple is invalid.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return ForbidClause(Errors.InvalidGrant, "The username/password couple is invalid.");

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var authorizations = await UserAuthorizationAsync(request, user, application);

            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
                return ForbidClause(Errors.ConsentRequired, "Access is forbidden for this user");

            return await PrincipalSignInAsync(user, request, application, authorizations);
        }

        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal) => claim.Type switch
        {
            //Claims.Name when principal.HasScope(Scopes.Profile) => FullClaimsDestinations,
            Claims.Email when principal.HasScope(Scopes.Email) => FullClaimsDestinations,
            Claims.Role when principal.HasScope(Scopes.Roles) => FullClaimsDestinations,
            Claims.Profile when principal.HasScope(Scopes.Profile) => FullClaimsDestinations,
            "secret_value" => [],
            _ => [Destinations.AccessToken]
        };

        private static void SetClaimDestinations(ClaimsPrincipal principal) =>
            principal.Claims.ToList().ForEach(claim => claim.SetDestinations(GetDestinations(claim, principal)));

        /// <summary>
        /// If the consent is implicit or if an authorization was found,
        /// return an authorization response without displaying the consent form.
        /// </summary>
        private async Task<IActionResult> PrincipalSignInAsync(User user, OpenIddictRequest request, object application, IEnumerable<object> authorizations)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            principal.SetScopes(BuildPrincipalScopePool(request.GetScopes()));
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            var authorization = authorizations.LastOrDefault();
            authorization ??= await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());

            SetClaimDestinations(principal);

            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Transforms principal scopes set.
        /// Note: the offline_access scope must be granted to allow OpenIddict to return a refresh token.
        /// </summary>
        private static IEnumerable<string> BuildPrincipalScopePool(ImmutableArray<string> requestScopes) => new[] {
                    Scopes.OpenId,
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.OfflineAccess,
                    Scopes.Roles
                }.Intersect(requestScopes);

        /// <summary>
        /// Retrieves the permanent authorizations associated with the user and the calling client application.
        /// </summary>
        private async Task<IEnumerable<object>> UserAuthorizationAsync(OpenIddictRequest request, User user, object application) => await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

        /// <summary>
        /// Checks token validity and expiration
        /// </summary>
        private static bool IsValidAndNotExpired(AuthenticateResult result, OpenIddictRequest request) =>
            result != null
            && result.Succeeded
            && (request.MaxAge == null
              || result.Properties?.IssuedUtc == null
              || (DateTimeOffset.UtcNow - result.Properties.IssuedUtc) > TimeSpan.FromSeconds(request.MaxAge.Value));

        private IActionResult InvalidAuthorizeRequestRedirect(OpenIddictRequest request)
        {
            if (request.HasPrompt(Prompts.None))
                return ForbidClause(Errors.LoginRequired, "The user is not logged in.");
            return ChallengeClause(Request.HasFormContentType ? [.. Request.Form] : Request.Query.ToList());
        }

        private ChallengeResult LoginRequestRedirect(OpenIddictRequest request)
        {
            var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

            var parameters = Request.HasFormContentType ?
                Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();
            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));
            return ChallengeClause(parameters);
        }

        private ChallengeResult ChallengeClause(IEnumerable<KeyValuePair<string, StringValues>> parameters) => Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                });

        private ForbidResult ForbidClause(string err, string description) => Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = err,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            })
        );
        #endregion
    }
}
