using AbstUI.Commands;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Sprites.Commands;

public abstract record BlingoSpriteBaseCommand(BlingoSpriteRef SpriteReference) : IAbstCommand;
