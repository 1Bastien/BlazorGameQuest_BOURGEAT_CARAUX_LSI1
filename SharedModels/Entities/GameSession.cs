using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels.Enums;

namespace SharedModels.Entities;

/// <summary>
/// Représente une partie de jeu d'un joueur.
/// Les salles sont générées aléatoirement au début de la partie et stockées en JSON.
/// </summary>
public class GameSession
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndTime { get; set; }
    
    public DateTime LastSaveTime { get; set; } = DateTime.UtcNow;
    
    public int Score { get; set; } = 0;
    
    [Range(0, 200)]
    public int CurrentHealth { get; set; } = 100;
    
    /// Index de la salle actuelle.
    [Range(1, 50)]
    public int CurrentRoomIndex { get; set; } = 0;
    
    /// Nombre total de salles générées pour cette partie.
    [Range(1, 50)]
    public int TotalRooms { get; set; }
    
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    
    /// Liste des salles générées pour cette partie (stockée en JSON).
    /// Contient les IDs des RoomTemplate utilisés pour chaque salle.
    [Required]
    public string GeneratedRoomsJson { get; set; } = "[]";
    
    /// Clé étrangère
    public Guid PlayerId { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual User Player { get; set; } = null!;
    
    /// Liste des actions effectuées par le joueur dans cette partie.
    public virtual ICollection<GameAction> Actions { get; set; } = new List<GameAction>();
    
}