using System;
using AbstUI.Resources;
using Blingo.PacMan.Core.Datas;

namespace Blingo.PacMan.Core.Engine;

/// <summary>
/// Persists Pac-Man specific player progress using the shared resource storage abstraction.
/// </summary>
public sealed class BlPacManRepository
{
    private const string _storageKey = "PacManGameState";
    private readonly IAbstResourceManager _resourceManager;

    public BlPacManRepository(IAbstResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }

    public BlPacManSaveData? Load()
    {
        return _resourceManager.StorageRead<BlPacManSaveData>(_storageKey);
    }

    public void Save(BlPacManSaveData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        _resourceManager.StorageWrite(_storageKey, data);
    }
}
