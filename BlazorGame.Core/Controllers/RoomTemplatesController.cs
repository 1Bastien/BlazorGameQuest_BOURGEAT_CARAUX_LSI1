using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion des modèles de salles de fouille
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoomTemplatesController : ControllerBase
{
    private readonly RoomTemplateService _service;

    public RoomTemplatesController(RoomTemplateService service)
    {
        _service = service;
    }

    /// Récupère tous les modèles de salles de fouille
    [HttpGet]
    public async Task<ActionResult<List<RoomTemplate>>> GetAll()
    {
        var templates = await _service.GetAllAsync();
        return Ok(templates);
    }

    /// Récupère un modèle de salle de fouille par son identifiant
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomTemplate>> GetById(Guid id)
    {
        var template = await _service.GetByIdAsync(id);
        if (template == null) return NotFound();
        
        return Ok(template);
    }

    /// Crée un nouveau modèle de salle de fouille
    [HttpPost]
    public async Task<ActionResult<RoomTemplate>> Create([FromBody] RoomTemplate template)
    {
        var created = await _service.CreateAsync(template);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// Met à jour un modèle de salle de fouille existant
    [HttpPut("{id}")]
    public async Task<ActionResult<RoomTemplate>> Update(Guid id, [FromBody] RoomTemplate template)
    {
        var updated = await _service.UpdateAsync(id, template);
        if (updated == null) return NotFound();
        
        return Ok(updated);
    }

    /// Supprime un modèle de salle de fouille
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        
        return NoContent();
    }
}
