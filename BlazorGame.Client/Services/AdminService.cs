using System.Net.Http.Json;
using SharedModels.Entities;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour gérer les appels API pour la page d'administration
/// </summary>
public class AdminService
{
    private readonly HttpClient _httpClient;

    public AdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region GameRewards

    /// <summary>
    /// Récupère la configuration des récompenses et pénalités
    /// </summary>
    public async Task<GameRewards?> GetGameRewardsAsync()
    {
        return await _httpClient.GetFromJsonAsync<GameRewards>("/api/GameRewards");
    }

    /// <summary>
    /// Met à jour la configuration des récompenses et pénalités
    /// </summary>
    public async Task<GameRewards?> UpdateGameRewardsAsync(GameRewards config)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/GameRewards", config);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameRewards>();
    }

    #endregion

    #region RoomTemplates

    /// <summary>
    /// Récupère tous les modèles de salles pour la page d'administration
    /// </summary>
    public async Task<List<RoomTemplate>?> GetAllRoomTemplatesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<RoomTemplate>>("/api/RoomTemplates/admin");
    }

    /// <summary>
    /// Crée un nouveau modèle de salle
    /// </summary>
    public async Task<RoomTemplate?> CreateRoomTemplateAsync(RoomTemplate template)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/RoomTemplates", template);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomTemplate>();
    }

    /// <summary>
    /// Met à jour un modèle de salle existant
    /// </summary>
    public async Task<RoomTemplate?> UpdateRoomTemplateAsync(Guid id, RoomTemplate template)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/RoomTemplates/{id}", template);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomTemplate>();
    }

    /// <summary>
    /// Supprime un modèle de salle
    /// </summary>
    public async Task<bool> DeleteRoomTemplateAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/RoomTemplates/{id}");
        return response.IsSuccessStatusCode;
    }

    #endregion
}

