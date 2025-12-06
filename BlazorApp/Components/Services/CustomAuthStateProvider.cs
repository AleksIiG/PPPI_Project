using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace practppi.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

                if (string.IsNullOrWhiteSpace(token))
                    return new AuthenticationState(_anonymous);

                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", token);
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);
            
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try 
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs != null)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        // 👇 ИСПРАВЛЕНИЕ: Проверяем тип JsonElement, чтобы избежать warning CS8604
                        if (kvp.Value is JsonElement element)
                        {
                            claims.Add(new Claim(kvp.Key, element.ToString()));
                        }
                        else
                        {
                            claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                        }
                    }
                }
            }
            catch { /* Игнорируем битые токены */ }
            
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}