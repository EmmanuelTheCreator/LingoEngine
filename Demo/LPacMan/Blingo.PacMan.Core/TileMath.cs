using System;

namespace Blingo.PacMan.Core;

/// <summary>
/// Provides helper routines for working with tile geometry.
/// </summary>
internal static class TileMath
{
    /// <summary>
    /// Calculates the Euclidean distance between two map tiles. When either tile is missing,
    /// the method returns <see cref="float.PositiveInfinity"/> so callers can ignore that option.
    /// </summary>
    public static float GetDistance(Tile? tileA, Tile? tileB)
    {
        if (tileA is null || tileB is null)
        {
            return float.PositiveInfinity;
        }

        var dx = tileA.CenterX - tileB.CenterX;
        var dy = tileA.CenterY - tileB.CenterY;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
