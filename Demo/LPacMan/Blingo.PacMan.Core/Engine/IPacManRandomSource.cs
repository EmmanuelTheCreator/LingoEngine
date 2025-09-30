using System;

namespace Blingo.PacMan.Core.Engine;

/// <summary>
/// Provides a deterministic-friendly abstraction around random number generation so tests can
/// substitute predictable sequences while gameplay keeps using a shared <see cref="Random"/>.
/// </summary>
internal interface IPacManRandomSource
{
    /// <summary>
    /// Returns a non-negative integer that is strictly less than <paramref name="exclusiveUpperBound"/>.
    /// </summary>
    int Next(int exclusiveUpperBound);
}

/// <summary>
/// Default implementation that delegates to <see cref="System.Random"/>.
/// </summary>
internal sealed class PacManRandomSource : IPacManRandomSource
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public int Next(int exclusiveUpperBound) => _random.Next(exclusiveUpperBound);
}
