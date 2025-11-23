namespace SharedModels.DTOs;

/// <summary>
/// DTO pour une entrée du classement
/// </summary>
public record LeaderboardEntry
{
    public Guid UserId { get; init; }
    public string? Username { get; set; }
    public int TotalScore { get; init; }
    public int TotalSessions { get; init; }
    public DateTime? LastSessionDate { get; init; }
}

