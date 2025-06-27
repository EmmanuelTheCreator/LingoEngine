namespace LingoEngine.Commands;

public interface ICommandHandler<TCommand> where TCommand : ILingoCommand
{
    bool CanExecute(TCommand command) => true;
    bool Handle(TCommand command);
}
