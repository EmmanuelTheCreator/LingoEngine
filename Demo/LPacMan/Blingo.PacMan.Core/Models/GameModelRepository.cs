using System;
using AbstUI.Resources;

namespace Blingo.PacMan.Core.Models;

/// <summary>
/// Persists Pac-Man specific player progress using the shared resource storage abstraction.
/// </summary>
public sealed class GameModelRepository : IGameModelRepository
{
    private const string StorageKey = "PacManGameState";
    private readonly IAbstResourceManager _resourceManager;

    public GameModelRepository(IAbstResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }

    public PacManSaveData? Load()
    {
        return _resourceManager.StorageRead<PacManSaveData>(StorageKey);
    }

    public void Save(PacManSaveData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        _resourceManager.StorageWrite(StorageKey, data);
    }
}

/// <summary>
/// Abstraction used by <see cref="GameModel"/> so tests can provide in-memory implementations.
/// </summary>
public interface IGameModelRepository
{
    PacManSaveData? Load();

    void Save(PacManSaveData data);
}

/// <summary>
/// Serializable payload stored in local storage for Pac-Man.
/// </summary>
public sealed class PacManSaveData
{
    public int HighScore { get; set; }
}
