using System.ComponentModel.DataAnnotations;

namespace SharedModels.Entities;

/// <summary>
/// Configuration globale des récompenses et pénalités du jeu, modifiable par les administrateurs.
/// Chaque récompense/pénalité a une plage de valeurs possibles.
/// Une valeur aléatoire dans cette plage sera choisie à chaque événement.
/// </summary>
public class GameRewards
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Range(50, 1000)]
    public int MinCombatVictoryPoints { get; set; } = 80;
    
    [Range(50, 1000)]
    public int MaxCombatVictoryPoints { get; set; } = 120;
    
    [Range(-1000, 0)]
    public int MinCombatDefeatPoints { get; set; } = -60;
    
    [Range(-1000, 0)]
    public int MaxCombatDefeatPoints { get; set; } = -40;
    
    [Range(-100, 0)]
    public int MinCombatDefeatHealthLoss { get; set; } = -40;
    
    [Range(-100, 0)]
    public int MaxCombatDefeatHealthLoss { get; set; } = -20;
    
    [Range(50, 1000)]
    public int MinTreasurePoints { get; set; } = 60;
    
    [Range(50, 1000)]
    public int MaxTreasurePoints { get; set; } = 90;
    
    [Range(20, 100)]
    public int MinPotionHealthGain { get; set; } = 30;
    
    [Range(20, 100)]
    public int MaxPotionHealthGain { get; set; } = 50;
    
    [Range(-1000, 0)]
    public int MinTrapPoints { get; set; } = -35;
    
    [Range(-1000, 0)]
    public int MaxTrapPoints { get; set; } = -15;
    
    [Range(-100, 0)]
    public int MinTrapHealthLoss { get; set; } = -30;
    
    [Range(-100, 0)]
    public int MaxTrapHealthLoss { get; set; } = -10;
    
    [Range(-1000, 0)]
    public int MinFleePoints { get; set; } = -20;
    
    [Range(-1000, 0)]
    public int MaxFleePoints { get; set; } = -10;
    
    [Range(1, 50)]
    public int MaxRooms { get; set; } = 5;
    
    [Range(50, 200)]
    public int StartingHealth { get; set; } = 100;
    
}
