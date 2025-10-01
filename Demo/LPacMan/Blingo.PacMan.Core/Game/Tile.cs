using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;
/// <summary>
/// Provides helper routines for working with tile geometry.
/// </summary>
internal static class TileMath
{
    public static int SpriteSize => BlPacManTheme.Actor.SpriteSize;
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

        var dx = tileA.X - tileB.X;
        var dy = tileA.Y - tileB.Y;
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

    public static float ClampVerticalPosition(float y, int mapHeight)
    {
        if (mapHeight <= 0)
            return y;

        if (y < 0)
            return 0f;

        var max = MathF.Max(0f, mapHeight - 1f);
        return y >= mapHeight ? max : y;
    }
}
public sealed class Tile
{
    public enum TileType
    {
        Unknown,
        Empty,
        Wall,
        Pellet,
        Pill,
        Tunnel,
        House,
    }
    public static int DefaultTileSize => BlPacManTheme.Tiles.Size;
    private static float VerticalCenterOffset => BlPacManTheme.Tiles.VerticalCenterOffset;


    public char Code { get; }
    public TileType Type { get; }

    public int Column { get; }

    public int Row { get; }

    public Map Map { get; }

    public int Width { get; }

    public int Height { get; }

    public float X { get; }

    public float Y { get; }

    internal BlPacManConsumableComponent? Item { get; set; }

    public Tile(char code, int column, int row, Map map)
    {
        Map = map;
        Code = code;
        Column = column;
        Row = row;
        Width = DefaultTileSize;
        Height = DefaultTileSize;
        X = Column * Width + Width / 2f;
        Y = Row * Height + Height / 2f + VerticalCenterOffset;
        Type = code switch
        {
            '=' => TileType.Wall,
            '.' => TileType.Pellet,
            '*' => TileType.Pill,
            't' => TileType.Tunnel,
            'h' => TileType.House,
            '-' => TileType.Empty,
            _ => TileType.Unknown,
        };
    }

    public bool IsWall() => Type == TileType.Wall;
    public bool IsEmpty() => Type == TileType.Empty;
    public bool IsHouse() => Type == TileType.House;
    public bool IsTunnel() => Type == TileType.Tunnel;

    public bool HasDot() => Item is not null && Type == TileType.Pellet;

    public bool HasPill() => Item is not null && Type == TileType.Pill;


    /// <summary>
    /// Determines whether the tile is part of the ghost house doorway, allowing
    /// ghosts to exit without re-entering from outside the maze.
    /// </summary>
    public bool IsGhostHouseEntrance()
    {
        var houseCenter = Map.HouseCenter;
        if (houseCenter is null)
            return false;

        if (ReferenceEquals(this, houseCenter))
            return true;

        var doorway = houseCenter.GetUp();
        var isDoorway = doorway is not null && ReferenceEquals(this, doorway);
        return isDoorway;
    }

  

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
