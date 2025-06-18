using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Commands;

internal sealed class CommandManager : ICommandManager
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, Type> _handlers = new();

    public CommandManager(IServiceProvider provider) => _provider = provider;

    public void Subscribe(Type handlerType)
    {
        foreach (var iface in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
        {
            var cmdType = iface.GetGenericArguments()[0];
            _handlers[cmdType] = handlerType;
        }
    }

    public bool Handle(ILingoCommand command)
    {
        var type = command.GetType();
        if (_handlers.TryGetValue(type, out var handlerType))
        {
            var handlerObj = _provider.GetService(handlerType) ?? ActivatorUtilities.CreateInstance(_provider, handlerType);
            dynamic h = handlerObj;
            dynamic c = command;
            if (h.CanExecute(c))
            {
                h.Handle(c);
                return true;
            }
        }
        return false;
    }
}
