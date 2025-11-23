namespace SharedModels.DTOs;

/// <summary>
/// DTO pour le statut d'un utilisateur
/// </summary>
public record UserStatusDto
{
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
}

