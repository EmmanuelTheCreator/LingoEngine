using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;

internal sealed class PacManCharacterOptions
{
    public float? Step { get; set; }

    public float? Speed { get; set; }

    public PacManDirection? Direction { get; set; }

    public bool? Preturn { get; set; }

    public string? Mode { get; set; }
}
