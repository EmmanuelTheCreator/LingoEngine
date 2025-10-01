using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;
/// <summary>
/// Provides helper routines for working with tile geometry.
/// </summary>
internal static class PMTileMath
{
    public static int SpriteSize => BlPacManTheme.Actor.SpriteSize;
    /// <summary>
    /// Calculates the Euclidean distance between two map tiles. When either tile is missing,
    /// the method returns <see cref="float.PositiveInfinity"/> so callers can ignore that option.
    /// </summary>
    public static float GetDistance(PMTile? tileA, PMTile? tileB)
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
    public static float GetMovementStep(PMMap? map)
    {
        var baseSize = map?.TileWidth ?? 0;
        if (baseSize <= 0 && map is not null)
        {
            baseSize = map.TileHeight;
        }

        if (baseSize <= 0)
        {
            baseSize = PMTile.DefaultTileSize;
        }

        return MathF.Max(1f, baseSize / 4f);
    }

   
}
public sealed class PMTile
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

    public PMMap Map { get; }

    public int Width { get; }

    public int Height { get; }

    public float X { get; }

    public float Y { get; }

    internal PMConsumableComponent? Item { get; set; }

    public PMTile(char code, int column, int row, PMMap map)
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

  

    public PMTile? Get(PMDirection direction)
    {
        return direction switch
        {
            PMDirection.Up => GetUp(),
            PMDirection.Down => GetDown(),
            PMDirection.Left => GetLeft(),
            PMDirection.Right => GetRight(),
            _ => null,
        };
    }

    public PMTile? GetUp() => Map.GetTile(Column, Row - 1);

    public PMTile? GetDown() => Map.GetTile(Column, Row + 1);

    public PMTile? GetLeft() => Map.GetTile(Column - 1, Row);

    public PMTile? GetRight() => Map.GetTile(Column + 1, Row);
}
