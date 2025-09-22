namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class IfTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "if"))
        {
            return false;
        }

        var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
        if (thenIndex < 0)
        {
            return false;
        }

        var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, thenIndex - 1);
        var condition = context.ConvertExpression(conditionTokens);
        context.Writer.WriteLine($"if ({condition})");
        context.Blocks.OpenBlock(BlockKind.If);
        return true;
    }
}
