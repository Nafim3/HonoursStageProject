using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Client.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("accessToken");
                if (string.IsNullOrWhiteSpace(token))
                    return _anonymous;

                if (IsTokenExpired(token))
                {
                    await _localStorage.RemoveItemAsync("accessToken");
                    return _anonymous;
                }

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return _anonymous;
            }
        }

        
        public async Task NotifyUserAuthentication(string token)
        {
            await _localStorage.SetItemAsync("accessToken", token);

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        
        public async Task NotifyUserLogout()
        {
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        private static bool IsTokenExpired(string jwt)
        {
            try
            {
                var payload = jwt.Split('.')[1];
                var json = DecodeBase64(payload);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("exp", out var expProp))
                {
                    long expSeconds;
                    if (expProp.ValueKind == JsonValueKind.Number && expProp.TryGetInt64(out expSeconds))
                    {
                        var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                        return expiry <= DateTimeOffset.UtcNow;
                    }

                    // sometimes exp could be string
                    if (expProp.ValueKind == JsonValueKind.String && long.TryParse(expProp.GetString(), out expSeconds))
                    {
                        var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                        return expiry <= DateTimeOffset.UtcNow;
                    }
                }

                
            }
            catch
            {
               
            }

            return true;
        }

        private static string DecodeBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try
            {
                var payload = jwt.Split('.')[1];
                var json = DecodeBase64(payload);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                foreach (var prop in root.EnumerateObject())
                {
                   
                    if (prop.NameEquals("exp")) continue;

                    string value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number => prop.Value.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => prop.Value.GetRawText()
                    };

                    claims.Add(new Claim(prop.Name, value));
                }

                
                if (!root.TryGetProperty("nameid", out _) && root.TryGetProperty("sub", out var subProp))
                {
                    var subVal = subProp.ValueKind == JsonValueKind.String ? subProp.GetString() ?? "" : subProp.GetRawText();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, subVal));
                }
            }
            catch
            {
                
            }

            return claims;
        }

        public static int ExtractUserId(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var json = DecodeBase64(payload);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try nameid
            if (root.TryGetProperty("nameid", out var idProp))
                return int.Parse(idProp.GetString()!);

            // Try sub
            if (root.TryGetProperty("sub", out var subProp))
                return int.Parse(subProp.GetString()!);

            throw new Exception("UserId not found in token");
        }



    }

}
