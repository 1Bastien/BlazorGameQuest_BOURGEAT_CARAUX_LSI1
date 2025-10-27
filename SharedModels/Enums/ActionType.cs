namespace SharedModels.Enums;

/// <summary>
/// Type d'action effectuée par le joueur dans une salle.
/// </summary>
public enum ActionType
{
    /// Combattre un monstre.
    Combat,

    /// Fouiller la salle.
    Search,
    
    /// Fuir la salle.
    Flee
}
