using System.Text.Json;
using BlazorGame.Core.Data;
using Microsoft.EntityFrameworkCore;
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

    /// Récupère toutes les sessions de jeu triées par date de début décroissante
    public async Task<List<GameSession>> GetAllAsync()
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .OrderByDescending(gs => gs.StartTime)
            .ToListAsync();
    }

    /// Récupère une session de jeu par son identifiant
    public async Task<GameSession?> GetByIdAsync(Guid id)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Actions)
            .FirstOrDefaultAsync(gs => gs.Id == id);
    }

    /// Crée une nouvelle session de jeu avec des salles générées aléatoirement
    public async Task<GameSession> CreateNewSessionAsync(Guid playerId)
    {
        var config = await _rewardsService.GetConfigAsync();
        if (config == null) throw new InvalidOperationException("GameRewards not configured");

        var roomCount = _random.Next(1, config.MaxRooms + 1);

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
}
