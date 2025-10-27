using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour la gestion de la configuration des récompenses et pénalités
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GameRewardsController : ControllerBase
{
    private readonly GameRewardsService _service;

    public GameRewardsController(GameRewardsService service)
    {
        _service = service;
    }

    /// Récupère la configuration des récompenses et pénalités
    [HttpGet]
    public async Task<ActionResult<GameRewards>> GetConfig()
    {
        var config = await _service.GetConfigAsync();
        if (config == null) return NotFound();
        
        return Ok(config);
    }

    /// Met à jour la configuration des récompenses et pénalités
    [HttpPut]
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
}
