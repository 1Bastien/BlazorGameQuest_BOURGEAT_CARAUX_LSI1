using BlazorGame.Core.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Core.Services;

/// <summary>
/// Service gérant les modèles de salles disponibles dans le jeu
/// </summary>
public class RoomTemplateService
{
    private readonly GameDbContext _context;

    public RoomTemplateService(GameDbContext context)
    {
        _context = context;
    }

    /// Récupère tous les modèles de salles actifs (pour le jeu)
    public async Task<List<RoomTemplate>> GetAllAsync()
    {
        return await _context.RoomTemplates
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    /// Récupère tous les modèles de salles (actifs et inactifs) pour l'administration
    public async Task<List<RoomTemplate>> GetAllIncludingInactiveAsync()
    {
        return await _context.RoomTemplates.ToListAsync();
    }

    /// Récupère un modèle de salle par son identifiant
    public async Task<RoomTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.RoomTemplates.FindAsync(id);
    }

    /// Crée un nouveau modèle de salle
    public async Task<RoomTemplate> CreateAsync(RoomTemplate template)
    {
        template.Id = Guid.NewGuid();
        
        _context.RoomTemplates.Add(template);
        await _context.SaveChangesAsync();
        
        return template;
    }

    /// Met à jour un modèle de salle existant
    public async Task<RoomTemplate?> UpdateAsync(Guid id, RoomTemplate template)
    {
        var existing = await _context.RoomTemplates.FindAsync(id);
        if (existing == null) return null;

        existing.Name = template.Name;
        existing.Description = template.Description;
        existing.Type = template.Type;
        existing.IsActive = template.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    /// Désactive un modèle de salle
    public async Task<bool> DesactivateAsync(Guid id)
    {
        var template = await _context.RoomTemplates.FindAsync(id);
        if (template == null) return false;

        template.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// Supprime un modèle de salle
    public async Task<bool> DeleteAsync(Guid id)
    {
        var template = await _context.RoomTemplates.FindAsync(id);
        if (template == null) return false;

        _context.RoomTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}
