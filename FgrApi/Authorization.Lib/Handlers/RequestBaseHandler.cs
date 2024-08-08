using System.Net.Http.Headers;
using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces.Options;
using IdentityModel.Client;

namespace Authorization.Lib.Handlers
{
    public abstract class RequestBaseHandler(IFgrApiOptions _config, string handlerScope) : DelegatingHandler
    {
        private async Task<TokenResponse> RequestClientCredentialsTokenAsync(ClientCredentialsTokenRequest request, CancellationToken cancellationToken = default)
        {
            var clone = request.Clone();

            clone.Parameters.AddRequired("grant_type", "client_credentials");
            clone.Parameters.AddOptional("scope", request.Scope);

            foreach (var resource in request.Resource)
            {
                clone.Parameters.AddRequired("resource", resource, allowDuplicates: true);
            }

            clone.Prepare();
            clone.Method = HttpMethod.Post;

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(clone, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ProtocolResponse.FromException<TokenResponse>(ex);
            }

            return await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(response).ConfigureAwait(false);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //potentially refresh token here if it has expired etc.
            var tokenResponse = await RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"{_config.BaseUrl}{FgrTermsHelper.tokenRoute}",
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                Scope = handlerScope
            }, cancellationToken).ConfigureAwait(false);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

    }
}
