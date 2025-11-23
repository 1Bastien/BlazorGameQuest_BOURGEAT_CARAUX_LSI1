using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DTOs;

namespace BlazorGame.Core.Controllers;

/// <summary>
/// Controller pour gérer le classement des joueurs
/// </summary>
[ApiController]
[Route("[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly UserService _userService;

    public LeaderboardController(UserService userService)
    {
        _userService = userService;
    }

    /// Récupère le classement des joueurs (sans filtrage admin, fait côté client)
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard()
    {
        var leaderboard = await _userService.GetLeaderboardAsync();
        return Ok(leaderboard);
    }
}

