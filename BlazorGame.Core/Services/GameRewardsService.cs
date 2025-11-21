using BlazorGame.Core.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Core.Services;

/// <summary>
/// Service gérant la configuration des récompenses et pénalités du jeu
/// </summary>
public class GameRewardsService
{
    private readonly GameDbContext _context;
    private static readonly Random _random = new();

    public GameRewardsService(GameDbContext context)
    {
        _context = context;
    }

    /// Récupère la configuration actuelle des récompenses
    public async Task<GameRewards?> GetConfigAsync()
    {
        return await _context.GameRewards.FirstOrDefaultAsync();
    }

    /// Met à jour la configuration des récompenses avec les nouvelles valeurs
    public async Task<GameRewards> UpdateConfigAsync(GameRewards config)
    {
        var existing = await GetConfigAsync()
                       ?? throw new InvalidOperationException("GameRewards configuration not found");

        var properties = typeof(GameRewards).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.CanWrite)
            {
                prop.SetValue(existing, prop.GetValue(config));
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    /// Génère un nombre aléatoire entre deux valeurs incluses
    public int GetRandomInRange(int min, int max) => _random.Next(min, max + 1);

    /// Crée une nouvelle configuration de récompenses
    public async Task<GameRewards> CreateConfigAsync(GameRewards config)
    {
        _context.GameRewards.Add(config);
        await _context.SaveChangesAsync();
        return config;
    }

    /// Supprime la configuration des récompenses
    public async Task<bool> DeleteConfigAsync()
    {
        var config = await GetConfigAsync();
        if (config == null) return false;

        _context.GameRewards.Remove(config);
        await _context.SaveChangesAsync();
        return true;
    }

    /// Récupère toutes les configurations de récompenses (normalement une seule)
    public async Task<List<GameRewards>> GetAllAsync()
    {
        return await _context.GameRewards.ToListAsync();
    }
}
