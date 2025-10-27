using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Core.Data;

/// <summary>
/// DbContext pour la base de données du jeu.
/// Sert à créer les entités et les relations entre elles.
/// </summary>
public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RoomTemplate> RoomTemplates { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<GameAction> GameActions { get; set; }
    public DbSet<GameRewards> GameRewards { get; set; }

    /// Configure les entités et les relations entre elles.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration User - GameSession (1-n)
        modelBuilder.Entity<GameSession>()
            .HasOne(gs => gs.Player)
            .WithMany(u => u.GameSessions)
            .HasForeignKey(gs => gs.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuration GameSession - GameAction (1-n)
        modelBuilder.Entity<GameAction>()
            .HasOne(ga => ga.GameSession)
            .WithMany(gs => gs.Actions)
            .HasForeignKey(ga => ga.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        GameDbContextSeeder.SeedData(modelBuilder);
    }
}
