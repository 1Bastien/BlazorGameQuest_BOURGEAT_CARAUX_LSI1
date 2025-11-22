using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion des actions de jeu
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class GameActionsController : ControllerBase
{
    private readonly GameActionService _service;

    public GameActionsController(GameActionService service)
    {
        _service = service;
    }

    /// Récupère toutes les actions de jeu
    [HttpGet]
    public async Task<ActionResult<List<GameAction>>> GetAll()
    {
        var actions = await _service.GetAllAsync();
        return Ok(actions);
    }

    /// Récupère une action par son identifiant
    [HttpGet("{id}")]
    public async Task<ActionResult<GameAction>> GetById(Guid id)
    {
        var action = await _service.GetByIdAsync(id);
        if (action == null) return NotFound();

        return Ok(action);
    }

    /// Crée une nouvelle action de jeu
    [HttpPost]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<GameAction>> Create([FromBody] GameAction action)
    {
        var created = await _service.CreateAsync(action);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// Met à jour une action de jeu existante
    [HttpPut("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<GameAction>> Update(Guid id, [FromBody] GameAction action)
    {
        var updated = await _service.UpdateAsync(id, action);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    /// Supprime une action de jeu
    [HttpDelete("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }
}

