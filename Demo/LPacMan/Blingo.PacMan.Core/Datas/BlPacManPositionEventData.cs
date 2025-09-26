using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Datas;

internal sealed class BlPacManPositionEventData
{
    public BlPacManPositionEventData(float x, float y, Tile? tile, BlPacManDirection direction)
    {
        X = x;
        Y = y;
        Tile = tile;
        Direction = direction;
    }

    public float X { get; }

    public float Y { get; }

    public Tile? Tile { get; }

    public BlPacManDirection Direction { get; }
}
