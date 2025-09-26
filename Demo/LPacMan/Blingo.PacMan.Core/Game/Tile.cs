using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;
/// <summary>
/// Provides helper routines for working with tile geometry.
/// </summary>
internal static class TileMath
{
    public static int SpriteSize = 16;
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

    /// <summary>
    /// Calculates the base per-frame movement step that keeps character speed aligned with the
    /// current map's tile size. When no map information is available, the default tile size is used.
    /// </summary>
    /// <param name="map">The active tile map.</param>
    /// <returns>The distance that should be traversed for a 100% speed setting.</returns>
    public static float GetMovementStep(Map? map)
    {
        var baseSize = map?.TileWidth ?? 0;
        if (baseSize <= 0 && map is not null)
        {
            baseSize = map.TileHeight;
        }

        if (baseSize <= 0)
        {
            baseSize = Tile.DefaultTileSize;
        }

        return MathF.Max(1f, baseSize / 4f);
    }
}
public sealed class Tile
{
    public const int DefaultTileSize = 16;
    private const float _verticalCenterOffset = 0f;

    public char Code { get; }

    public int Column { get; }

    public int Row { get; }

    public Map Map { get; }

    public int Width { get; }

    public int Height { get; }

    public float CenterX { get; }

    public float CenterY { get; }

    internal BlPacManConsumableComponent? Item { get; set; }

    public Tile(char code, int column, int row, Map map)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Code = code;
        Column = column;
        Row = row;
        Width = DefaultTileSize;
        Height = DefaultTileSize;
        CenterX = Column * Width + Width / 2f;
        CenterY = Row * Height + Height / 2f + _verticalCenterOffset;
    }

    public bool IsWall() => Code == '=';

    public bool IsHouse() => Code == 'h';

    public bool IsTunnel() => Code == 't';

    public bool HasDot() => Item is not null && Code == '.';

    public bool HasPill() => Item is not null && Code == '*';

    public Tile? Get(BlPacManDirection direction)
    {
        return direction switch
        {
            BlPacManDirection.Up => GetUp(),
            BlPacManDirection.Down => GetDown(),
            BlPacManDirection.Left => GetLeft(),
            BlPacManDirection.Right => GetRight(),
            _ => null,
        };
    }

    public Tile? GetUp() => Map.GetTile(Column, Row - 1);

    public Tile? GetDown() => Map.GetTile(Column, Row + 1);

    public Tile? GetLeft() => Map.GetTile(Column - 1, Row);

    public Tile? GetRight() => Map.GetTile(Column + 1, Row);
}
