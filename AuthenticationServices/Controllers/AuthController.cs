using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthenticationServices.Controllers;

/// <summary>
/// Controller pour gérer l'authentification via Keycloak
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Authentifie un utilisateur avec Keycloak et retourne un token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"];

        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId!),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password)
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Déconnecte un utilisateur en révoquant son refresh token
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"];

        var logoutEndpoint = $"{authority}/protocol/openid-connect/logout";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId!),
            new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
        });

        await _httpClient.PostAsync(logoutEndpoint, content);

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Rafraîchit un token JWT expiré en utilisant le refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"];

        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", clientId!),
            new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Récupère les informations de l'utilisateur connecté depuis Keycloak
    /// </summary>
    [HttpGet("userinfo")]
    public async Task<ActionResult> GetUserInfo()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "Missing or invalid token" });
        }

        var token = authHeader.Substring("Bearer ".Length);
        var authority = _configuration["Keycloak:Authority"];
        var userInfoEndpoint = $"{authority}/protocol/openid-connect/userinfo";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync(userInfoEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var userInfo = await response.Content.ReadAsStringAsync();
        return Ok(JsonSerializer.Deserialize<JsonElement>(userInfo));
    }
}

public record LoginRequest(string Username, string Password);

public record LogoutRequest(string RefreshToken);

public record RefreshRequest(string RefreshToken);

public class TokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public int refresh_expires_in { get; set; }
    public string token_type { get; set; } = string.Empty;
}

