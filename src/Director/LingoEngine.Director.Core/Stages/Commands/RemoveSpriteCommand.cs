using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Stages.Commands;

/// <summary>
/// Command for deleting a sprite from the score.
/// </summary>
public sealed record RemoveSpriteCommand(LingoMovie Movie, LingoSprite Sprite) : ILingoCommand;

