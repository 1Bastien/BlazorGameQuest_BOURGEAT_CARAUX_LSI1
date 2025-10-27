namespace SharedModels.Enums;

/// <summary>
/// Résultat d'une action effectuée par le joueur dans une salle.
/// </summary>
public enum GameActionResult
{
    /// Victoire contre le monstre.
    Victory,

    /// Défaite contre le monstre.
    Defeat,

    /// Trésor trouvé.
    FoundTreasure,

    /// Potion trouvée.
    FoundPotion,

    /// Piège déclenché.
    TriggeredTrap,

    /// Fuite de la salle.
    Escaped
}
