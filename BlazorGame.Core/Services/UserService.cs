using Microsoft.EntityFrameworkCore;
using BlazorGame.Core.Data;
using SharedModels.Entities;
using SharedModels.DTOs;

namespace BlazorGame.Core.Services;

/// <summary>
/// Service pour gérer les utilisateurs du jeu
/// Les utilisateurs sont créés automatiquement depuis Keycloak
/// </summary>
public class UserService
{
    private readonly GameDbContext _context;

    public UserService(GameDbContext context)
    {
        _context = context;
    }

    /// Vérifie si un utilisateur existe et le crée si nécessaire
    public async Task<User> EnsureUserExistsAsync(Guid userId, string? username = null, string? role = null)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            user = new User
            {
                Id = userId,
                Username = username,
                Role = role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            bool needsUpdate = false;

            // Mettre à jour le username s'il était vide
            if (string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(username))
            {
                user.Username = username;
                needsUpdate = true;
            }

            // Mettre à jour le rôle s'il a changé
            if (!string.IsNullOrEmpty(role) && user.Role != role)
            {
                user.Role = role;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _context.SaveChangesAsync();
            }
        }

        return user;
    }

    /// Récupère un utilisateur par son ID
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// Récupère tous les utilisateurs
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.GameSessions)
            .ToListAsync();
    }

    /// Récupère le classement des joueurs avec leur score total
    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        var leaderboard = await _context.Users
            .Include(u => u.GameSessions)
            .Select(u => new LeaderboardEntry
            {
                UserId = u.Id,
                Username = u.Username,
                TotalScore = u.GameSessions
                    .Where(s => s.Status == SharedModels.Enums.GameStatus.Completed ||
                                s.Status == SharedModels.Enums.GameStatus.Failed)
                    .Sum(s => s.Score),
                TotalSessions = u.GameSessions
                    .Where(s => s.Status == SharedModels.Enums.GameStatus.Completed ||
                                s.Status == SharedModels.Enums.GameStatus.Failed)
                    .Count(),
                LastSessionDate = u.GameSessions
                    .Where(s => s.Status == SharedModels.Enums.GameStatus.Completed ||
                                s.Status == SharedModels.Enums.GameStatus.Failed)
                    .OrderByDescending(s => s.StartTime)
                    .Select(s => (DateTime?)s.StartTime)
                    .FirstOrDefault()
            })
            .Where(l => l.TotalSessions > 0)
            .OrderByDescending(l => l.TotalScore)
            .ToListAsync();

        return leaderboard;
    }

    /// Récupère tous les joueurs avec leurs informations pour l'administration
    public async Task<List<PlayerAdminDto>> GetAllPlayersForAdminAsync()
    {
        var players = await _context.Users
            .Include(u => u.GameSessions)
            .Select(u => new PlayerAdminDto
            {
                UserId = u.Id,
                Username = u.Username,
                LastConnectionDate = u.GameSessions
                    .OrderByDescending(s => s.StartTime)
                    .Select(s => (DateTime?)s.StartTime)
                    .FirstOrDefault(),
                TotalGamesPlayed = u.GameSessions.Count(),
                TotalScore = u.GameSessions
                    .Where(s => s.Status == SharedModels.Enums.GameStatus.Completed ||
                                s.Status == SharedModels.Enums.GameStatus.Failed)
                    .Sum(s => s.Score),
                IsActive = u.IsActive,
                Role = u.Role
            })
            .ToListAsync();

        // Trier : d'abord par dernière connexion (les plus récents en premier), 
        // puis par nom d'utilisateur pour les joueurs sans parties
        return players
            .OrderByDescending(p => p.LastConnectionDate.HasValue)
            .ThenByDescending(p => p.LastConnectionDate)
            .ThenBy(p => p.Username)
            .ToList();
    }

    /// Récupère toutes les sessions d'un joueur spécifique
    public async Task<List<GameSession>> GetPlayerSessionsAsync(Guid playerId)
    {
        return await _context.GameSessions
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    /// Supprime toutes les sessions d'un joueur (reset de l'historique)
    public async Task<bool> DeletePlayerSessionsAsync(Guid playerId)
    {
        var sessions = await _context.GameSessions
            .Where(s => s.PlayerId == playerId)
            .ToListAsync();

        if (sessions.Count == 0)
            return false;

        _context.GameSessions.RemoveRange(sessions);
        await _context.SaveChangesAsync();
        return true;
    }

    /// Active ou désactive un utilisateur
    public async Task<User?> ToggleUserActiveStatusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return user;
    }
}

