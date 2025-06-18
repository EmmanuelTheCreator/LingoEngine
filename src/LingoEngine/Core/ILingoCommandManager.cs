using LingoEngine.Commands;

namespace LingoEngine.Core;

public interface ILingoCommandManager
{
    public void Register<THandler, TCommand>()
        where THandler : ICommandHandler<TCommand>
        where TCommand : ILingoCommand;
    void Register(Type handlerType);
    bool Handle(ILingoCommand command);
}
