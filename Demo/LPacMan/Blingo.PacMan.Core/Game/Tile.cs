using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;

public sealed class Tile
{
    private const int DefaultTileSize = 32;
    private const float VerticalCenterOffset = 4f;

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
        CenterY = Row * Height + Height / 2f + VerticalCenterOffset;
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
