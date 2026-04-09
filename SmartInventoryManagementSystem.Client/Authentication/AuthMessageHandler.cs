using Blazored.LocalStorage;
using SmartInventoryManagementSystem.Application.DTO.AuthDTO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Client.Authentication
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _clientFactory;

        public AuthMessageHandler(ILocalStorageService localStorage, IHttpClientFactory clientFactory)
        {
            _localStorage = localStorage;
            _clientFactory = clientFactory;
        }


        protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var userId = CustomAuthStateProvider.ExtractUserId(token);

                    var client = _clientFactory.CreateClient("API");

                    var refreshResponse = await client.PostAsJsonAsync(
                        "api/auth/refresh",
                        new RefreshTokenRequest
                        {
                            RefreshToken = refreshToken,
                            UserId = userId
                        });

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var newTokens =
                            await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

                        if (newTokens != null)
                        {
                            await _localStorage.SetItemAsync("accessToken", newTokens.AccessToken);
                            await _localStorage.SetItemAsync("refreshToken", newTokens.RefreshToken);

                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);

                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }
            }

            return response;
        }


    }
}
