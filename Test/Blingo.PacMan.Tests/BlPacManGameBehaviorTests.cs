using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstUI.Resources;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class BlPacManGameBehaviorTests
{
    [Fact]
    public void GameModel_follows_default_mode_sequence()
    {
        var repository = new GameModelRepository(new InMemoryResourceManager());
        var model = new GameModel(repository);
        var recorded = new List<GhostMode>();

        model.SubscribeModeChanged(mode =>
        {
            if (mode is GhostMode value)
            {
                recorded.Add(value);
            }
        });

        model.UpdateMode();
        AdvanceSeconds(model, 7);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 7);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 5);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 5);

        var expected = new List<GhostMode>
        {
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
        };

        recorded.Should().Equal(expected);
    }

    private static void AdvanceSeconds(GameModel model, int seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        var frames = seconds * 60;
        for (var i = 0; i < frames; i++)
        {
            model.UpdateMode();
        }
    }

    private sealed class InMemoryResourceManager : IAbstResourceManager
    {
        private readonly Dictionary<string, object> _storage = new(StringComparer.OrdinalIgnoreCase);

        public string ProjectFolder { get; set; } = string.Empty;

        public bool FileExists(string fileName) => false;

        public Task<bool> FileExistsAsync(string fileName) => Task.FromResult(false);

        public string? ReadTextFile(string fileName) => null;

        public Task<string?> ReadTextFileAsync(string fileName) => Task.FromResult<string?>(null);

        public byte[]? ReadBytes(string fileName) => null;

        public Task<byte[]?> ReadBytesAsync(string fileName) => Task.FromResult<byte[]?>(null);

        public void StorageWrite<T>(string key, T data)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _storage[key] = data!;
        }

        public Task StorageWriteAsync<T>(string key, T data)
        {
            StorageWrite(key, data);
            return Task.CompletedTask;
        }

        public T? StorageRead<T>(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_storage.TryGetValue(key, out var value))
            {
                return (T?)value;
            }

            return default;
        }

        public Task<T?> StorageReadAsync<T>(string key)
        {
            return Task.FromResult(StorageRead<T>(key));
        }

        public string Serialize<T>(T data) => throw new NotSupportedException();

        public T? Deserialize<T>(string content) => throw new NotSupportedException();
    }
}
