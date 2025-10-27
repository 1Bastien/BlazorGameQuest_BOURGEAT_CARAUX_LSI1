using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels.Enums;

namespace SharedModels.Entities;

/// <summary>
/// Représente une action effectuée par le joueur dans une salle.
/// </summary>
public class GameAction
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    public ActionType Type { get; set; }
    
    [Required]
    public GameActionResult Result { get; set; }
    
    public int PointsChange { get; set; }
    
    public int HealthChange { get; set; }
    
    [Range(1, 5)]
    public int RoomNumber { get; set; }
    
    public Guid GameSessionId { get; set; }
    
    [ForeignKey("GameSessionId")]
    public virtual GameSession GameSession { get; set; } = null!;
}
