using LingoEngine.Commands;

namespace LingoEngine.Core;

public interface ICommandHandler<TCommand> where TCommand : ILingoCommand
{
    bool CanExecute(TCommand command) => true;
    bool Handle(TCommand command);
}
