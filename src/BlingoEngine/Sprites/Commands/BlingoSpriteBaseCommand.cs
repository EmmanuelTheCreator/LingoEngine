using AbstUI.Commands;
using BlingoEngine.Sprites;

namespace BlingoEngine.Sprites.Commands;

public abstract record BlingoSpriteBaseCommand(BlingoSpriteRef SpriteReference) : IAbstCommand;
