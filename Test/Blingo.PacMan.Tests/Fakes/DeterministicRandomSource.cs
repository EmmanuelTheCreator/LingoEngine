using System;
using Blingo.PacMan.Core.Engine;

namespace Blingo.PacMan.Tests.Fakes;

/// <summary>
/// Deterministic implementation of <see cref="IPacManRandomSource"/> that always returns the
/// configured value so frightened mode decisions can be asserted.
/// </summary>
internal sealed class DeterministicRandomSource : IPacManRandomSource
{
    private readonly int _value;

    /// <summary>
    /// Initializes a new instance that will always return <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to return from <see cref="Next"/>. Defaults to zero.</param>
    public DeterministicRandomSource(int value = 0)
    {
        _value = value;
    }

    /// <inheritdoc />
    public int Next(int exclusiveUpperBound)
    {
        if (exclusiveUpperBound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
        }

        return _value % exclusiveUpperBound;
    }
}
