using Microsoft.EntityFrameworkCore;
using BlazorGame.Core.Data;
using SharedModels.Entities;

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
    public async Task<User> EnsureUserExistsAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            user = new User { Id = userId };
            _context.Users.Add(user);
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
}

