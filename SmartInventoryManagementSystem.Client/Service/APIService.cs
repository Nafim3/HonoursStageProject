using Blazored.LocalStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Client.Service
{
    public class APIService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public APIService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<HttpClient> GetAuthorizedClient()
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);

            return _http;
        }
    }
}