namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class ExitNextTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "exit"))
        {
            if (tokens.Count >= 3 &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat") &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = context.ConvertExpression(conditionTokens);
                context.Writer.WriteLine($"if ({condition}) break;");
                return true;
            }

            if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
            {
                context.Writer.WriteLine("break;");
                return true;
            }

            if (tokens.Count == 1)
            {
                context.Writer.WriteLine("return;");
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "next") &&
            tokens.Count >= 2 &&
            BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
        {
            if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = context.ConvertExpression(conditionTokens);
                context.Writer.WriteLine($"if ({condition}) continue;");
                return true;
            }

            context.Writer.WriteLine("continue;");
            return true;
        }

        return false;
    }
}
