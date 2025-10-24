using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.ParentScripts;

namespace Blingo.PacMan.Core.Datas;

internal sealed class BlPacManCharacterOptions
{
    public float? Step { get; set; }

    public float? Speed { get; set; }

    public PMDirection Direction { get; set; }

    public bool? Preturn { get; set; }

    public GhostMode Mode { get; set; }
    public PMTile? StartTile { get; set; }
    
}
