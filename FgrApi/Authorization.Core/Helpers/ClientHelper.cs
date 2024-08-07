using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.Core.Helpers
{
    public static class ClientHelper
    {
        public static async Task AddWebClient(
            IOpenIddictApplicationManager manager,
            IEnumerable<Uri> inUris,
            IEnumerable<Uri> outUris,
            CancellationToken cancellationToken)
        {
            var descriptor = new OpenIddictApplicationDescriptor()
            {
                ClientId = AuthParams.AuthParams.webClientId,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = AuthParams.AuthParams.webClientDisplayName,
                PostLogoutRedirectUris = { },
                RedirectUris = { },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Scopes.Email,
                    Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                    Permissions.Prefixes.GrantType + "sign_in_as"
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            };

            inUris.ToList().ForEach(uri => descriptor.RedirectUris.Add(uri));
            outUris.ToList().ForEach(uri => descriptor.PostLogoutRedirectUris.Add(uri));
            await RenewClientApp(AuthParams.AuthParams.webClientId, manager, descriptor, cancellationToken);
        }

        public static async Task AddPostmanClient(IOpenIddictApplicationManager manager, CancellationToken cancellationToken)
        {
            await RenewClientApp("postman", manager, new OpenIddictApplicationDescriptor
            {
                ClientId = "postman",
                ConsentType = ConsentTypes.Systematic,
                DisplayName = "Postman",
                RedirectUris =
                {
                    new Uri("urn:postman")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Device,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.DeviceCode,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Scopes.Phone
                }
            }, cancellationToken);
        }

        public static async Task AddResourceServerClient(IOpenIddictApplicationManager manager, CancellationToken cancellationToken)
        {
            await RenewClientApp("server", manager, new OpenIddictApplicationDescriptor
            {
                ClientId = "server",
                ClientSecret = AuthParams.AuthParams.webClientSecret,
                DisplayName = "Resource server",
                Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                    }
            }, cancellationToken);
        }

        private static async Task RenewClientApp(
            string appId,
            IOpenIddictApplicationManager manager,
            OpenIddictApplicationDescriptor applicationDescriptor,
            CancellationToken cancellationToken)
        {
            var client = await manager.FindByClientIdAsync(appId, cancellationToken);
            if (client != null)
                await manager.DeleteAsync(client, cancellationToken);
            await manager.CreateAsync(applicationDescriptor, cancellationToken);
        }

    }
}
