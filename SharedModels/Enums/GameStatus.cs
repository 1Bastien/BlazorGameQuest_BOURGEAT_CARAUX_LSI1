namespace SharedModels.Enums;

/// <summary>
/// État d'une partie.
/// </summary>
public enum GameStatus
{
    /// Partie en cours, peut être reprise.
    InProgress,
    
    /// Partie terminée avec succès.
    Completed,
    
    /// Partie terminée par mort (0 PV).
    Failed,
    
    /// Partie abandonnée par le joueur.
    Abandoned
}
