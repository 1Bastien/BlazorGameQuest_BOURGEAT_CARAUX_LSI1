using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;
using SharedModels.Enums;
using System.Security.Claims;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion des sessions de jeu
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class GameSessionsController : ControllerBase
{
    private readonly GameSessionService _sessionService;
    private readonly GameActionService _actionService;
    private readonly UserService _userService;

    public GameSessionsController(
        GameSessionService sessionService,
        GameActionService actionService,
        UserService userService)
    {
        _sessionService = sessionService;
        _actionService = actionService;
        _userService = userService;
    }

    /// Récupère une session de jeu par son identifiant
    [HttpGet("{id}")]
    public async Task<ActionResult<GameSession>> GetById(Guid id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session == null) return NotFound();

        return Ok(session);
    }


    /// Crée une nouvelle session de jeu
    [HttpPost("start")]
    public async Task<ActionResult<GameSession>> StartNewGame([FromBody] StartGameRequest request)
    {
        // Extraire le username depuis le token JWT
        string? username = null;
        if (User?.Identity?.IsAuthenticated == true)
        {
            username = User.FindFirst("preferred_username")?.Value;
        }

        // S'assurer que l'utilisateur existe avec son username
        await _userService.EnsureUserExistsAsync(request.PlayerId, username);

        var session = await _sessionService.CreateNewSessionAsync(request.PlayerId);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    /// Effectue une action dans une session de jeu
    [HttpPost("{sessionId}/action")]
    public async Task<ActionResult<GameActionResponse>> PerformAction(Guid sessionId,
        [FromBody] PerformActionRequest request)
    {
        var session = await _sessionService.GetByIdAsync(sessionId);
        if (session == null) return NotFound("Session not found");

        if (!_sessionService.CanContinue(session))
            return BadRequest("Session cannot continue");

        var action =
            await _actionService.ProcessActionAsync(sessionId, request.ActionType, session.CurrentRoomIndex + 1);

        session = await _sessionService.GetByIdAsync(sessionId);

        return Ok(new GameActionResponse
        {
            Action = action,
            UpdatedSession = session!
        });
    }

    /// Abandonne une session de jeu
    [HttpPost("{sessionId}/abandon")]
    public async Task<ActionResult> AbandonSession(Guid sessionId)
    {
        var result = await _sessionService.AbandonSessionAsync(sessionId);
        if (!result) return NotFound();

        return NoContent();
    }

    /// Récupère toutes les actions d'une session de jeu
    [HttpGet("{sessionId}/actions")]
    public async Task<ActionResult<List<GameAction>>> GetSessionActions(Guid sessionId)
    {
        var actions = await _actionService.GetSessionActionsAsync(sessionId);
        return Ok(actions);
    }

    /// Récupère toutes les sessions d'un joueur (historique complet)
    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<List<GameSession>>> GetPlayerSessions(Guid playerId)
    {
        var sessions = await _sessionService.GetPlayerSessionsAsync(playerId);
        return Ok(sessions);
    }

    /// Récupère la session en cours d'un joueur (s'il y en a une)
    [HttpGet("player/{playerId}/current")]
    public async Task<ActionResult<GameSession>> GetPlayerCurrentSession(Guid playerId)
    {
        var session = await _sessionService.GetPlayerCurrentSessionAsync(playerId);
        if (session == null) return NotFound();

        return Ok(session);
    }

    /// Récupère toutes les sessions de jeu
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<GameSession>>> GetAll()
    {
        var sessions = await _sessionService.GetAllAsync();
        return Ok(sessions);
    }

    /// Met à jour une session de jeu
    [HttpPut("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<GameSession>> Update(Guid id, [FromBody] GameSession session)
    {
        var updated = await _sessionService.UpdateSessionAsync(id, session);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    /// Supprime une session de jeu
    [HttpDelete("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _sessionService.DeleteAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }
}

/// DTO pour créer une nouvelle session de jeu
public record StartGameRequest(Guid PlayerId);

/// DTO pour effectuer une action dans une session de jeu
public record PerformActionRequest(ActionType ActionType);

/// DTO pour la réponse d'une action dans une session de jeu
public record GameActionResponse
{
    public GameAction Action { get; init; } = null!;
    public GameSession UpdatedSession { get; init; } = null!;
}
