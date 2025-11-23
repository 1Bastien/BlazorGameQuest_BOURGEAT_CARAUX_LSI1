using System.Net.Http.Json;
using System.Text.Json;
using SharedModels.Entities;
using SharedModels.Enums;
using SharedModels.DTOs;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour gérer les appels API vers le backend de jeu
/// </summary>
public class GameService
{
    private readonly HttpClient _httpClient;

    public GameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Démarre une nouvelle session de jeu
    /// </summary>
    public async Task<GameSession?> StartNewGameAsync(Guid playerId)
    {
        var request = new StartGameRequest(playerId);
        var response = await _httpClient.PostAsJsonAsync("/api/GameSessions/start", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GameSession>();
        }

        return null;
    }

    /// <summary>
    /// Effectue une action dans la session de jeu
    /// </summary>
    public async Task<GameActionResponse?> PerformActionAsync(Guid sessionId, ActionType actionType)
    {
        var request = new PerformActionRequest(actionType);
        var response = await _httpClient.PostAsJsonAsync($"/api/GameSessions/{sessionId}/action", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GameActionResponse>();
        }

        return null;
    }

    /// <summary>
    /// Abandonne une session de jeu
    /// </summary>
    public async Task<bool> AbandonSessionAsync(Guid sessionId)
    {
        var response = await _httpClient.PostAsync($"/api/GameSessions/{sessionId}/abandon", null);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Récupère un modèle de salle par son ID
    /// </summary>
    public async Task<RoomTemplate?> GetRoomTemplateAsync(Guid templateId)
    {
        return await _httpClient.GetFromJsonAsync<RoomTemplate>($"/api/RoomTemplates/{templateId}");
    }

    /// <summary>
    /// Récupère toutes les sessions d'un joueur (historique)
    /// </summary>
    public async Task<List<GameSession>?> GetPlayerSessionsAsync(Guid playerId)
    {
        return await _httpClient.GetFromJsonAsync<List<GameSession>>($"/api/GameSessions/player/{playerId}");
    }

    /// <summary>
    /// Récupère la session en cours d'un joueur (s'il y en a une)
    /// </summary>
    public async Task<GameSession?> GetPlayerCurrentSessionAsync(Guid playerId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GameSession>($"/api/GameSessions/player/{playerId}/current");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Récupère la liste des IDs de salles depuis le JSON de la session
    /// </summary>
    public List<Guid> GetGeneratedRoomIds(string generatedRoomsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(generatedRoomsJson) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    /// <summary>
    /// Récupère la configuration des récompenses du jeu
    /// </summary>
    public async Task<GameRewards?> GetGameRewardsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GameRewards>("/api/GameRewards");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Récupère le classement des joueurs
    /// </summary>
    public async Task<List<LeaderboardEntry>?> GetLeaderboardAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<LeaderboardEntry>>("/api/Leaderboard");
        }
        catch
        {
            return null;
        }
    }

}

/// <summary>
/// DTO pour démarrer une nouvelle session
/// </summary>
public record StartGameRequest(Guid PlayerId);

/// <summary>
/// DTO pour effectuer une action
/// </summary>
public record PerformActionRequest(ActionType ActionType);

/// <summary>
/// DTO de réponse après une action
/// </summary>
public record GameActionResponse
{
    public GameAction Action { get; init; } = null!;
    public GameSession UpdatedSession { get; init; } = null!;
}


