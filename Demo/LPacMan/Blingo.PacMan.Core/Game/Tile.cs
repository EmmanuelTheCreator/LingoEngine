using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;
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
public sealed class Tile
{
    private const int _defaultTileSize = 16;
    private const float _verticalCenterOffset = 2f;

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
        Width = _defaultTileSize;
        Height = _defaultTileSize;
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
