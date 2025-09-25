using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Datas;

internal sealed class BlPacManTileEventData
{
    public BlPacManTileEventData(Tile tile)
    {
        Tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }

    public Tile Tile { get; }
}
