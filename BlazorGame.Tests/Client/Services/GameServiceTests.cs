using System.Net;
using System.Net.Http.Json;
using BlazorGame.Client.Services;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service GameService
/// </summary>
public class GameServiceTests
{
    /// Crée un HttpClient avec un handler de test personnalisé
    private HttpClient CreateTestHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    /// Test: StartNewGameAsync démarre une nouvelle session avec succès
    [Fact]
    public async Task StartNewGameAsync_ReturnsGameSession_WhenSuccessful()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var expectedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Score = 0,
            CurrentHealth = 100,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/GameSessions/start", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedSession)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.StartNewGameAsync(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSession.Id, result.Id);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(GameStatus.InProgress, result.Status);
    }

    /// Test: PerformActionAsync effectue une action avec succès
    [Fact]
    public async Task PerformActionAsync_ReturnsGameActionResponse_WhenSuccessful()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var actionType = ActionType.Combat;

        var expectedResponse = new GameActionResponse
        {
            Action = new GameAction
            {
                Id = Guid.NewGuid(),
                GameSessionId = sessionId,
                Type = actionType,
                Result = GameActionResult.Victory,
                PointsChange = 15,
                HealthChange = 0,
                RoomNumber = 1,
                Timestamp = DateTime.UtcNow
            },
            UpdatedSession = new GameSession
            {
                Id = sessionId,
                Score = 15,
                CurrentHealth = 100,
                CurrentRoomIndex = 1,
                Status = GameStatus.InProgress,
                TotalRooms = 5,
                GeneratedRoomsJson = "[]"
            }
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"/api/GameSessions/{sessionId}/action", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResponse)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.PerformActionAsync(sessionId, actionType);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Action);
        Assert.NotNull(result.UpdatedSession);
        Assert.Equal(actionType, result.Action.Type);
        Assert.Equal(sessionId, result.UpdatedSession.Id);
    }

    /// Test: AbandonSessionAsync abandonne une session avec succès
    [Fact]
    public async Task AbandonSessionAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"/api/GameSessions/{sessionId}/abandon", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.AbandonSessionAsync(sessionId);

        // Assert
        Assert.True(result);
    }

    /// Test: GetRoomTemplateAsync récupère un template avec succès
    [Fact]
    public async Task GetRoomTemplateAsync_ReturnsRoomTemplate_WhenSuccessful()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var expectedTemplate = new RoomTemplate
        {
            Id = templateId,
            Name = "Test Room",
            Description = "Test Description",
            Type = RoomType.Combat,
            IsActive = true
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/api/RoomTemplates/{templateId}", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedTemplate)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.GetRoomTemplateAsync(templateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(templateId, result.Id);
        Assert.Equal(expectedTemplate.Name, result.Name);
    }

    /// Test: GetPlayerSessionsAsync récupère toutes les sessions d'un joueur
    [Fact]
    public async Task GetPlayerSessionsAsync_ReturnsPlayerSessions_WhenSuccessful()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var expectedSessions = new List<GameSession>
        {
            new GameSession
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Score = 100,
                Status = GameStatus.Completed,
                TotalRooms = 5,
                GeneratedRoomsJson = "[]"
            },
            new GameSession
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Score = 50,
                Status = GameStatus.InProgress,
                TotalRooms = 5,
                GeneratedRoomsJson = "[]"
            }
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/api/GameSessions/player/{playerId}", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedSessions)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.GetPlayerSessionsAsync(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(playerId, s.PlayerId));
    }

    /// Test: GetPlayerCurrentSessionAsync récupère la session en cours
    [Fact]
    public async Task GetPlayerCurrentSessionAsync_ReturnsCurrentSession_WhenSuccessful()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var expectedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Score = 50,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/api/GameSessions/player/{playerId}/current", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedSession)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.GetPlayerCurrentSessionAsync(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSession.Id, result.Id);
        Assert.Equal(GameStatus.InProgress, result.Status);
    }

    /// Test: GetGeneratedRoomIds désérialise correctement le JSON
    [Fact]
    public void GetGeneratedRoomIds_ReturnsListOfGuids_WhenValidJson()
    {
        // Arrange
        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        var roomId1 = Guid.NewGuid();
        var roomId2 = Guid.NewGuid();
        var json = $"[\"{roomId1}\",\"{roomId2}\"]";

        // Act
        var result = service.GetGeneratedRoomIds(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(roomId1, result);
        Assert.Contains(roomId2, result);
    }

    /// Test: GetGameRewardsAsync récupère la configuration avec succès
    [Fact]
    public async Task GetGameRewardsAsync_ReturnsGameRewards_WhenSuccessful()
    {
        // Arrange
        var expectedConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            MinCombatVictoryPoints = 80,
            MaxCombatVictoryPoints = 120,
            MinCombatDefeatPoints = -60,
            MaxCombatDefeatPoints = -40,
            MinCombatDefeatHealthLoss = -40,
            MaxCombatDefeatHealthLoss = -20,
            MinTreasurePoints = 60,
            MaxTreasurePoints = 90,
            MinPotionHealthGain = 30,
            MaxPotionHealthGain = 50,
            MinTrapPoints = -35,
            MaxTrapPoints = -15,
            MinTrapHealthLoss = -30,
            MaxTrapHealthLoss = -10,
            MinFleePoints = -20,
            MaxFleePoints = -10,
            NumberOfRooms = 5,
            StartingHealth = 100
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/GameRewards", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedConfig)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new GameService(httpClient);

        // Act
        var result = await service.GetGameRewardsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedConfig.MinCombatVictoryPoints, result.MinCombatVictoryPoints);
        Assert.Equal(expectedConfig.MaxCombatVictoryPoints, result.MaxCombatVictoryPoints);
        Assert.Equal(expectedConfig.StartingHealth, result.StartingHealth);
        Assert.Equal(expectedConfig.NumberOfRooms, result.NumberOfRooms);
    }
}

