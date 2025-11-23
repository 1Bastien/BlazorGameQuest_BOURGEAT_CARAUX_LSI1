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
    public async Task<User> EnsureUserExistsAsync(Guid userId, string? username = null)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            user = new User
            {
                Id = userId,
                Username = username
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(username))
        {
            // Mettre à jour le username s'il était vide
            user.Username = username;
            await _context.SaveChangesAsync();
        }

        return user;
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
}

