using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BlazorGame.Core.Data;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Core.Services;

/// <summary>
/// Service gérant les sessions de jeu et leur progression
/// </summary>
public class GameSessionService
{
    private readonly GameDbContext _context;
    private readonly RoomTemplateService _roomService;
    private readonly GameRewardsService _rewardsService;
    private static readonly Random _random = new();

    public GameSessionService(
        GameDbContext context,
        RoomTemplateService roomService,
        GameRewardsService rewardsService)
    {
        _context = context;
        _roomService = roomService;
        _rewardsService = rewardsService;
    }

    /// Récupère une session de jeu par son identifiant
    public async Task<GameSession?> GetByIdAsync(Guid id)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Actions)
            .FirstOrDefaultAsync(gs => gs.Id == id);
    }

    /// Crée une nouvelle session de jeu avec un nombre fixe de salles générées aléatoirement
    public async Task<GameSession> CreateNewSessionAsync(Guid playerId)
    {
        var config = await _rewardsService.GetConfigAsync();
        if (config == null) throw new InvalidOperationException("GameRewards not configured");

        var roomCount = config.NumberOfRooms;

        var allTemplates = await _roomService.GetAllAsync();
        if (allTemplates.Count == 0) throw new InvalidOperationException("No room templates available");
        var selectedRoomIds = new List<Guid>();
        for (int i = 0; i < roomCount; i++)
        {
            var randomTemplate = allTemplates[_random.Next(allTemplates.Count)];
            selectedRoomIds.Add(randomTemplate.Id);
        }

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            StartTime = DateTime.UtcNow,
            LastSaveTime = DateTime.UtcNow,
            Score = 0,
            CurrentHealth = config.StartingHealth,
            CurrentRoomIndex = 0,
            TotalRooms = roomCount,
            GeneratedRoomsJson = JsonSerializer.Serialize(selectedRoomIds),
            Status = GameStatus.InProgress
        };

        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    /// Met à jour l'état d'une session de jeu existante
    public async Task<GameSession?> UpdateSessionAsync(Guid id, GameSession session)
    {
        var existing = await _context.GameSessions.FindAsync(id);
        if (existing == null) return null;

        existing.Score = session.Score;
        existing.CurrentHealth = session.CurrentHealth;
        existing.CurrentRoomIndex = session.CurrentRoomIndex;
        existing.Status = session.Status;
        existing.LastSaveTime = DateTime.UtcNow;

        if (session.EndTime.HasValue)
            existing.EndTime = session.EndTime;

        await _context.SaveChangesAsync();
        return existing;
    }

    /// Marque une session comme abandonnée et enregistre la date de fin
    public async Task<bool> AbandonSessionAsync(Guid sessionId)
    {
        var session = await _context.GameSessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = GameStatus.Abandoned;
        session.EndTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// Vérifie si une session peut continuer en fonction de son état
    public bool CanContinue(GameSession session)
    {
        return session.Status == GameStatus.InProgress
               && session.CurrentHealth > 0
               && session.CurrentRoomIndex < session.TotalRooms;
    }

    /// Récupère toutes les sessions d'un joueur triées par date décroissante
    public async Task<List<GameSession>> GetPlayerSessionsAsync(Guid playerId)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Where(gs => gs.PlayerId == playerId)
            .OrderByDescending(gs => gs.StartTime)
            .ToListAsync();
    }

    /// Récupère la session en cours d'un joueur
    public async Task<GameSession?> GetPlayerCurrentSessionAsync(Guid playerId)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Actions)
            .Where(gs => gs.PlayerId == playerId && gs.Status == GameStatus.InProgress)
            .OrderByDescending(gs => gs.LastSaveTime)
            .FirstOrDefaultAsync();
    }

    /// Récupère toutes les sessions de jeu
    public async Task<List<GameSession>> GetAllAsync()
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Actions)
            .OrderByDescending(gs => gs.StartTime)
            .ToListAsync();
    }

    /// Supprime une session de jeu
    public async Task<bool> DeleteAsync(Guid id)
    {
        var session = await _context.GameSessions.FindAsync(id);
        if (session == null) return false;

        _context.GameSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }
}
