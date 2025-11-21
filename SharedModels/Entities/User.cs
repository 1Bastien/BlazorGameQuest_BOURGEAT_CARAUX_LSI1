using System.ComponentModel.DataAnnotations;

namespace SharedModels.Entities;

/// <summary>
/// Représente un utilisateur du jeu (joueur ou administrateur).
/// Stocke uniquement l'ID Keycloak pour référencer l'utilisateur.
/// </summary>
public class User
{
    /// <summary>
    /// ID de l'utilisateur provenant de Keycloak.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Liste des parties de jeu associées à l'utilisateur.
    /// </summary>
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
