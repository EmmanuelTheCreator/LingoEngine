using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Commands;

internal sealed class LingoCommandManager : ILingoCommandManager
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, Type> _handlers = new();

    public LingoCommandManager(IServiceProvider provider) => _provider = provider;

    public void Register<THandler, TCommand>()
        where THandler : ICommandHandler<TCommand>
        where TCommand : ILingoCommand
        => Register(typeof(THandler));

    public void Register(Type handlerType)
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
            var handlerObj = _provider.GetService(handlerType);// ?? ActivatorUtilities.CreateInstance(_provider, handlerType);
            if (handlerObj == null)
                throw new Exception("Handler not found for command: " + type.FullName);
            dynamic h = handlerObj;
            dynamic c = command;
            if (h.CanExecute(c))
            {
                return h.Handle(c);
            }
        }
        return false;
    }
}
