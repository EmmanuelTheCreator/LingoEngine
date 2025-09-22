namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class ElseTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "else"))
        {
            return false;
        }

        if (tokens.Count > 1 && BlLegacyHandlerTokenUtilities.IsKeyword(tokens[1], "if"))
        {
            var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
            if (thenIndex < 0)
            {
                return false;
            }

            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, thenIndex - 2);
            var condition = context.ConvertExpression(conditionTokens);
            context.Blocks.CloseBlock(leaveOnStack: true);
            context.Writer.WriteLine($"else if ({condition})");
            context.Blocks.OpenBlock(BlockKind.If, reopenExisting: true);
            return true;
        }

        context.Blocks.CloseBlock(leaveOnStack: true);
        context.Writer.WriteLine("else");
        context.Blocks.OpenBlock(BlockKind.If, reopenExisting: true);
        return true;
    }
}
