using BlazorGame.Core.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Core.Services;

/// <summary>
/// Service responsable de la gestion des actions du joueur dans une session de jeu
/// </summary>
public class GameActionService
{
    private readonly GameDbContext _context;
    private readonly GameRewardsService _rewardsService;
    private static readonly Random _random = new();

    public GameActionService(GameDbContext context, GameRewardsService rewardsService)
    {
        _context = context;
        _rewardsService = rewardsService;
    }

    /// Traite une action du joueur et met à jour l'état de la session de jeu
    public async Task<GameAction> ProcessActionAsync(Guid sessionId, ActionType actionType, int roomNumber)
    {
        var config = await _rewardsService.GetConfigAsync();
        if (config == null) throw new InvalidOperationException("GameRewards not configured");

        var session = await _context.GameSessions.FindAsync(sessionId);
        if (session == null) throw new InvalidOperationException("Session not found");

        var (result, pointsChange, healthChange) = DetermineActionResult(actionType, config);

        var action = new GameAction
        {
            Id = Guid.NewGuid(),
            GameSessionId = sessionId,
            Type = actionType,
            Result = result,
            PointsChange = pointsChange,
            HealthChange = healthChange,
            RoomNumber = roomNumber,
            Timestamp = DateTime.UtcNow
        };

        session.Score += pointsChange;
        session.CurrentHealth = Math.Max(0, Math.Min(200, session.CurrentHealth + healthChange));
        session.CurrentRoomIndex++;
        session.LastSaveTime = DateTime.UtcNow;

        if (session.CurrentHealth <= 0)
        {
            session.Status = GameStatus.Failed;
            session.EndTime = DateTime.UtcNow;
        }
        else if (session.CurrentRoomIndex >= session.TotalRooms)
        {
            session.Status = GameStatus.Completed;
            session.EndTime = DateTime.UtcNow;
        }

        _context.GameActions.Add(action);
        await _context.SaveChangesAsync();

        return action;
    }

    /// Détermine le résultat d'une action en fonction de son type
    private (GameActionResult result, int points, int health) DetermineActionResult(ActionType actionType,
        in GameRewards config)
    {
        return actionType switch
        {
            ActionType.Combat => DetermineCombatResult(config),
            ActionType.Search => DetermineSearchResult(config),
            ActionType.Flee => (GameActionResult.Escaped,
                _rewardsService.GetRandomInRange(config.MinFleePoints, config.MaxFleePoints), 0),
            _ => throw new ArgumentException("Invalid action type")
        };
    }

    /// Détermine le résultat d'un combat avec 50% de chance de victoire
    private (GameActionResult result, int points, int health) DetermineCombatResult(in GameRewards config)
    {
        if (_random.Next(100) < 50)
        {
            return (GameActionResult.Victory,
                _rewardsService.GetRandomInRange(config.MinCombatVictoryPoints, config.MaxCombatVictoryPoints), 0);
        }

        return (GameActionResult.Defeat,
            _rewardsService.GetRandomInRange(config.MinCombatDefeatPoints, config.MaxCombatDefeatPoints),
            _rewardsService.GetRandomInRange(config.MinCombatDefeatHealthLoss, config.MaxCombatDefeatHealthLoss));
    }

    /// Détermine le résultat d'une recherche avec 33% trésor, 33% potion, 33% piège
    private (GameActionResult result, int points, int health) DetermineSearchResult(in GameRewards config)
    {
        var roll = _random.Next(100);
        if (roll < 33)
        {
            return (GameActionResult.FoundTreasure,
                _rewardsService.GetRandomInRange(config.MinTreasurePoints, config.MaxTreasurePoints), 0);
        }

        if (roll < 66)
        {
            return (GameActionResult.FoundPotion, 0,
                _rewardsService.GetRandomInRange(config.MinPotionHealthGain, config.MaxPotionHealthGain));
        }

        return (GameActionResult.TriggeredTrap,
            _rewardsService.GetRandomInRange(config.MinTrapPoints, config.MaxTrapPoints),
            _rewardsService.GetRandomInRange(config.MinTrapHealthLoss, config.MaxTrapHealthLoss));
    }

    /// Récupère toutes les actions d'une session de jeu triées par ordre chronologique
    public async Task<List<GameAction>> GetSessionActionsAsync(Guid sessionId)
    {
        return await _context.GameActions
            .Where(a => a.GameSessionId == sessionId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync();
    }

    /// Récupère une action par son identifiant
    public async Task<GameAction?> GetByIdAsync(Guid id)
    {
        return await _context.GameActions
            .Include(a => a.GameSession)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// Récupère toutes les actions de jeu
    public async Task<List<GameAction>> GetAllAsync()
    {
        return await _context.GameActions
            .Include(a => a.GameSession)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    /// Supprime une action de jeu
    public async Task<bool> DeleteAsync(Guid id)
    {
        var action = await _context.GameActions.FindAsync(id);
        if (action == null) return false;

        _context.GameActions.Remove(action);
        await _context.SaveChangesAsync();
        return true;
    }

    /// Crée une nouvelle action de jeu manuellement
    public async Task<GameAction> CreateAsync(GameAction action)
    {
        action.Id = Guid.NewGuid();
        action.Timestamp = DateTime.UtcNow;

        _context.GameActions.Add(action);
        await _context.SaveChangesAsync();

        return action;
    }

    /// Met à jour une action de jeu existante
    public async Task<GameAction?> UpdateAsync(Guid id, GameAction action)
    {
        var existing = await _context.GameActions.FindAsync(id);
        if (existing == null) return null;

        existing.Type = action.Type;
        existing.Result = action.Result;
        existing.PointsChange = action.PointsChange;
        existing.HealthChange = action.HealthChange;
        existing.RoomNumber = action.RoomNumber;

        await _context.SaveChangesAsync();
        return existing;
    }
}
