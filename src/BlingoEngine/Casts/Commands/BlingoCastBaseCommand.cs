using AbstUI.Commands;
using BlingoEngine.Casts;

namespace BlingoEngine.Casts.Commands;

public abstract record BlingoCastBaseCommand(BlingoCastRef CastReference) : IAbstCommand;
