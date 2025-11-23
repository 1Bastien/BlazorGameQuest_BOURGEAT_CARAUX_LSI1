using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DTOs;
using SharedModels.Entities;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion des utilisateurs
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// Récupère tous les joueurs pour l'administration
    [HttpGet("admin")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<List<PlayerAdminDto>>> GetAllPlayersForAdmin()
    {
        var players = await _userService.GetAllPlayersForAdminAsync();
        return Ok(players);
    }

    /// Récupère toutes les sessions d'un joueur spécifique
    [HttpGet("{userId}/sessions")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<List<GameSession>>> GetPlayerSessions(Guid userId)
    {
        var sessions = await _userService.GetPlayerSessionsAsync(userId);
        return Ok(sessions);
    }

    /// Supprime toutes les sessions d'un joueur (reset de l'historique)
    [HttpDelete("{userId}/sessions")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult> DeletePlayerSessions(Guid userId)
    {
        var result = await _userService.DeletePlayerSessionsAsync(userId);
        if (!result)
            return NotFound("Aucune session trouvée pour ce joueur");

        return NoContent();
    }

    /// Active ou désactive un utilisateur
    [HttpPatch("{userId}/toggle-active")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<User>> ToggleUserActiveStatus(Guid userId)
    {
        var user = await _userService.ToggleUserActiveStatusAsync(userId);
        if (user == null)
            return NotFound("Utilisateur non trouvé");

        return Ok(user);
    }

    /// Vérifie le statut actif d'un utilisateur (utilisé lors de la connexion)
    [HttpGet("me/status")]
    public async Task<ActionResult<UserStatusDto>> GetMyStatus()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        
        return Ok(new UserStatusDto 
        { 
            IsActive = user?.IsActive ?? true,
            UserId = userId
        });
    }
}

