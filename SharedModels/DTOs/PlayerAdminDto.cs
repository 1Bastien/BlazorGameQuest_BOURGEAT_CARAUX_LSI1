namespace SharedModels.DTOs;

/// <summary>
/// DTO pour afficher les informations d'un joueur dans l'administration
/// </summary>
public record PlayerAdminDto
{
    public Guid UserId { get; init; }
    public string? Username { get; init; }
    public DateTime? LastConnectionDate { get; init; }
    public int TotalGamesPlayed { get; init; }
    public int TotalScore { get; init; }
    public bool IsActive { get; init; }
    public string? Role { get; init; }
}

