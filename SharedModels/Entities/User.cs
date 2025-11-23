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
    /// Nom d'utilisateur provenant de Keycloak.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Rôle de l'utilisateur provenant de Keycloak (joueur ou administrateur).
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Indique si le compte utilisateur est actif.
    /// Un compte désactivé ne peut pas se connecter.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date et heure de la dernière connexion de l'utilisateur.
    /// </summary>
    public DateTime? LastConnectionDate { get; set; }

    /// <summary>
    /// Liste des parties de jeu associées à l'utilisateur.
    /// </summary>
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
