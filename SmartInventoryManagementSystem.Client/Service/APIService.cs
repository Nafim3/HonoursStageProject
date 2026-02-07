using Blazored.LocalStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


// this class is responsible for providing an HttpClient instance with the JWT token included in the Authorization header for authenticated API requests.
// It retrieves the token from local storage and sets it in the HttpClient's headers before returning the client for use in making API calls.
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