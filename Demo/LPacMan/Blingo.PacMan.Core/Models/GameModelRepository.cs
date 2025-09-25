using System;
using AbstUI.Resources;
using Blingo.PacMan.Core.Datas;

namespace Blingo.PacMan.Core.Models;

/// <summary>
/// Persists Pac-Man specific player progress using the shared resource storage abstraction.
/// </summary>
public sealed class GameModelRepository
{
    private const string StorageKey = "PacManGameState";
    private readonly IAbstResourceManager _resourceManager;

    public GameModelRepository(IAbstResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }

    public BlPacManSaveData? Load()
    {
        return _resourceManager.StorageRead<BlPacManSaveData>(StorageKey);
    }

    public void Save(BlPacManSaveData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        _resourceManager.StorageWrite(StorageKey, data);
    }
}
