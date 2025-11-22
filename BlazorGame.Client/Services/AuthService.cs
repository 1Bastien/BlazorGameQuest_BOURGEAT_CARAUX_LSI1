using System.Net.Http.Json;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour gérer l'authentification côté client
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    /// <summary>
    /// Connecte un utilisateur et stocke les tokens dans le LocalStorage
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new { Username = username, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse != null)
            {
                await _localStorage.SetItemAsStringAsync("access_token", tokenResponse.access_token);
                await _localStorage.SetItemAsStringAsync("refresh_token", tokenResponse.refresh_token);
                SetAuthorizationHeader(tokenResponse.access_token);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Déconnecte l'utilisateur et supprime les tokens du LocalStorage
    /// </summary>
    public async Task LogoutAsync()
    {
        var refreshToken = await _localStorage.GetItemAsStringAsync("refresh_token");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var request = new { RefreshToken = refreshToken };
            await _httpClient.PostAsJsonAsync("/api/auth/logout", request);
        }

        await _localStorage.RemoveItemAsync("access_token");
        await _localStorage.RemoveItemAsync("refresh_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Vérifie si l'utilisateur est authentifié et si son token est valide
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("access_token");
        if (string.IsNullOrEmpty(token))
            return false;

        // Vérifier si le token est expiré
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Essayer de rafraîchir le token
                return await RefreshTokenAsync();
            }

            SetAuthorizationHeader(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Rafraîchit le token JWT en utilisant le refresh token
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await _localStorage.GetItemAsStringAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var request = new { RefreshToken = refreshToken };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", request);

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse != null)
            {
                await _localStorage.SetItemAsStringAsync("access_token", tokenResponse.access_token);
                await _localStorage.SetItemAsStringAsync("refresh_token", tokenResponse.refresh_token);
                SetAuthorizationHeader(tokenResponse.access_token);
                return true;
            }
        }

        await LogoutAsync();
        return false;
    }

    /// <summary>
    /// Récupère l'ID de l'utilisateur depuis le token JWT (claim 'sub')
    /// </summary>
    public async Task<Guid?> GetUserIdAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("access_token");
        if (string.IsNullOrEmpty(token))
            return null;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            {
                return userId;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Vérifie si l'utilisateur a le rôle administrateur
    /// </summary>
    public async Task<bool> IsAdminAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("access_token");
        if (string.IsNullOrEmpty(token))
            return false;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            var realmAccessClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "realm_access");
            if (realmAccessClaim != null)
            {
                var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.GetString() == "administrateur")
                            return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Configure le header Authorization avec le token JWT
    /// </summary>
    private void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

public class TokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public int refresh_expires_in { get; set; }
    public string token_type { get; set; } = string.Empty;
}

