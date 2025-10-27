using System.ComponentModel.DataAnnotations;
using SharedModels.Enums;

namespace SharedModels.Entities;

/// <summary>
/// Modèle de salle créé par l'administrateur. Ces salles sont mises dans la collection 
/// d'une GameSession pour générer les salles de la partie.
/// L'administrateur peut créer differentes salles avec differentes descriptions et types.
/// En fonction du type de la salle, les points et la vie gagnés ou perdus sont differents.
/// Les gains et pertes sont générés aléatoirement dans une plage de valeurs définie dans la table GameRewards par l'administrateur.
/// </summary>
public class RoomTemplate
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public RoomType Type { get; set; }
    
    /// Indique si la salle est activée par l'administrateur pour être utilisée dans une partie.
    public bool IsActive { get; set; } = true;
}
