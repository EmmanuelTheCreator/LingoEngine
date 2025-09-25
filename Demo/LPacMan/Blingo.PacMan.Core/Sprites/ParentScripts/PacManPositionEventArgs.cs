using System;
using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;

internal sealed class PacManPositionEventArgs : EventArgs
{
    public PacManPositionEventArgs(float x, float y, Tile? tile, PacManDirection direction)
    {
        X = x;
        Y = y;
        Tile = tile;
        Direction = direction;
    }

    public float X { get; }

    public float Y { get; }

    public Tile? Tile { get; }

    public PacManDirection Direction { get; }
}
