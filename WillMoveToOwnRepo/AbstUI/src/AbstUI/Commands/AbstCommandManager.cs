using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstUI.Commands;

public sealed class AbstCommandManager : IAbstCommandManager
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public AbstCommandManager(IServiceProvider provider) => _provider = provider;

    public IAbstCommandManager Register<THandler, TCommand>(bool replace = false)
        where THandler : IAbstCommandHandler<TCommand>
        where TCommand : IAbstCommand
        => Register(typeof(THandler), replace);

    public IAbstCommandManager Register(Type handlerType, bool replace = false)
    {
        foreach (var iface in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAbstCommandHandler<>)))
        {
            var cmdType = iface.GetGenericArguments()[0];
            if (!_handlers.TryGetValue(cmdType, out var handlers) || replace)
            {
                _handlers[cmdType] = new List<Type> { handlerType };
                continue;
            }

            if (!handlers.Contains(handlerType))
            {
                handlers.Add(handlerType);
            }
        }
        return this;
    }

    public bool Handle(IAbstCommand command)
    {
        var type = command.GetType();
        var ok = false;
        if (_handlers.TryGetValue(type, out var handlerTypes))
        {
            foreach (var handlerType in handlerTypes)
            {
                var handlerObj = _provider.GetService(handlerType);
                if (handlerObj == null)
                    throw new Exception("Handler not found for command: " + type.FullName);
                dynamic h = handlerObj;
                dynamic c = command;
                if (h.CanExecute(c))
                {
                    ok = h.Handle(c);
                    if (!ok) return false;
                }
            }
            return ok;
        }
        return false;
    }

    public void Clear(string? preserveNamespaceFragment = null)
    {
        if (string.IsNullOrEmpty(preserveNamespaceFragment))
        {
            _handlers.Clear();
            return;
        }

        foreach (var cmdType in _handlers.Keys.ToList())
        {
            var list = _handlers[cmdType];
            list.RemoveAll(t => t.Namespace == null ||
                t.Namespace.IndexOf(preserveNamespaceFragment, StringComparison.OrdinalIgnoreCase) < 0);
            if (list.Count == 0)
                _handlers.Remove(cmdType);
        }
    }

    public IAbstCommandManager Preload<TCommand>() where TCommand : IAbstCommand
    {
        var type = typeof(TCommand);
        if (!_handlers.TryGetValue(type, out var handlerTypes))
            return this;
        foreach (var handlerType in handlerTypes)
            _provider.GetService(handlerType);
        return this;
    }
}
