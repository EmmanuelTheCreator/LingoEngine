namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class CaseTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "case"))
        {
            var ofIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "of");
            if (ofIndex < 0)
            {
                return false;
            }

            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, ofIndex - 1);
            var expression = context.ConvertExpression(expressionTokens);
            context.Writer.WriteLine($"switch ({expression})");
            context.Blocks.OpenBlock(BlockKind.Switch);
            return true;
        }

        if (!context.Blocks.IsCurrent(BlockKind.Switch))
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "when"))
        {
            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, tokens.Count - 1);
            var expression = context.ConvertExpression(expressionTokens);
            context.Writer.WriteLine($"case {expression}:");
            context.Blocks.StartSwitchSection();
            return true;
        }

        if (tokens.Count == 1 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "otherwise"))
        {
            context.Writer.WriteLine("default:");
            context.Blocks.StartSwitchSection();
            return true;
        }

        return false;
    }
}
