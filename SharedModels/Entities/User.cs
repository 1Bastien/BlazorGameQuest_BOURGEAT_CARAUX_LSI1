using System.ComponentModel.DataAnnotations;
using SharedModels.Enums;

namespace SharedModels.Entities;

/// <summary>
/// Représente un utilisateur du jeu (joueur ou administrateur).
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; } = UserRole.Player;
    
    /// Indique si le compte est actif (peut être banni par l'administrateur).
    public bool IsActive { get; set; } = true;
    
    /// Liste des parties de jeu associées à l'utilisateur.
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
