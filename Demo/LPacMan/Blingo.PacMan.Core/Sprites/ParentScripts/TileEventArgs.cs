using System;
using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;

internal sealed class TileEventArgs : EventArgs
{
    public TileEventArgs(Tile tile)
    {
        Tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }

    public Tile Tile { get; }
}
