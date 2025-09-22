namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class RepeatTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "repeat"))
        {
            return false;
        }

        if (tokens.Count >= 4 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "with"))
        {
            var variableToken = tokens[2];
            var variableName = BlCSharpName.SanitizeIdentifier(variableToken.ValueText);
            var inIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "in");
            if (inIndex > 0)
            {
                var sourceTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, inIndex + 1, tokens.Count - (inIndex + 1));
                var source = context.ConvertExpression(sourceTokens);
                context.Writer.WriteLine($"foreach (var {variableName} in {source})");
                context.Blocks.OpenBlock(BlockKind.Loop);
                return true;
            }

            var toIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "to");
            var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
            if (equalsIndex > 0 && toIndex > equalsIndex)
            {
                var startTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, toIndex - equalsIndex - 1);
                var endTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, toIndex + 1, tokens.Count - (toIndex + 1));
                var start = context.ConvertExpression(startTokens);
                var end = context.ConvertExpression(endTokens);
                context.Writer.WriteLine($"for (int {variableName} = {start}; {variableName} <= {end}; {variableName}++)");
                context.Blocks.OpenBlock(BlockKind.Loop);
                return true;
            }

            return false;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "while"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = context.ConvertExpression(conditionTokens);
            context.Writer.WriteLine($"while ({condition})");
            context.Blocks.OpenBlock(BlockKind.Loop);
            return true;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "until"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = context.ConvertExpression(conditionTokens);
            context.Writer.WriteLine("do");
            context.Blocks.OpenBlock(BlockKind.RepeatUntil, condition);
            return true;
        }

        if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "forever"))
        {
            context.Writer.WriteLine("while (true)");
            context.Blocks.OpenBlock(BlockKind.Loop);
            return true;
        }

        return false;
    }
}
