namespace BlingoEngine.Legacy.Lingo.CodeGen;

public interface IHandlerTokenVisitor
{
    bool TryHandle(HandlerTokenEmitContext context);
}
