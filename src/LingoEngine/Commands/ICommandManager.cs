namespace LingoEngine.Commands;

public interface ICommandManager
{
    void Subscribe(Type handlerType);
    bool Handle(ILingoCommand command);
}
