using AbstUI.Commands;
using BlingoEngine.Casts;

namespace BlingoEngine.Director.Core.Casts.Commands;

public abstract record BlingoCastBaseCommand(BlingoCastRef CastReference) : IAbstCommand;
