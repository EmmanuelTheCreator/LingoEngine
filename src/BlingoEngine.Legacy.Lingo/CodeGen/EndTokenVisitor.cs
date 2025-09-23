namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class EndTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        if (context.Tokens.Count == 0 ||
            !BlLegacyHandlerTokenUtilities.IsKeyword(context.Tokens[0], "end"))
        {
            return false;
        }

        context.Blocks.CloseBlock();
        return true;
    }
}
