using System;
namespace AbstUI.Commands;

public interface IAbstCommandManager
{
    IAbstCommandManager Register<THandler, TCommand>(bool replace = false)
        where THandler : IAbstCommandHandler<TCommand>
        where TCommand : IAbstCommand;
    IAbstCommandManager Register(Type handlerType, bool replace = false);
    bool Handle(IAbstCommand command);
    void Clear(string? preserveNamespaceFragment = null);
    /// <summary>
    /// Services that need to be preloaded and resolved to subscribe to the command handler.
    /// </summary>
    IAbstCommandManager Preload<TCommand>() where TCommand : IAbstCommand;
}
