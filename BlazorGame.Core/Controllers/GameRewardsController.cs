using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion de la configuration des récompenses et pénalités
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class GameRewardsController : ControllerBase
{
    private readonly GameRewardsService _service;

    public GameRewardsController(GameRewardsService service)
    {
        _service = service;
    }

    /// Récupère la configuration des récompenses et pénalités
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<GameRewards>> GetConfig()
    {
        var config = await _service.GetConfigAsync();
        if (config == null) return NotFound();

        return Ok(config);
    }

    /// Met à jour la configuration des récompenses et pénalités
    [HttpPut]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<GameRewards>> UpdateConfig([FromBody] GameRewards config)
    {
        try
        {
            var updated = await _service.UpdateConfigAsync(config);
            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// Crée une nouvelle configuration des récompenses
    [HttpPost]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<GameRewards>> CreateConfig([FromBody] GameRewards config)
    {
        var created = await _service.CreateConfigAsync(config);
        return CreatedAtAction(nameof(GetConfig), created);
    }

    /// Supprime la configuration des récompenses
    [HttpDelete]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult> DeleteConfig()
    {
        var result = await _service.DeleteConfigAsync();
        if (!result) return NotFound();

        return NoContent();
    }

    /// Récupère toutes les configurations
    [HttpGet("all")]
    public async Task<ActionResult<List<GameRewards>>> GetAll()
    {
        var configs = await _service.GetAllAsync();
        return Ok(configs);
    }
}
