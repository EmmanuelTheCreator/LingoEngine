namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class ExpressionStatementVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var expression = context.ConvertExpression(context.Tokens);
        if (expression.Length == 0)
        {
            return false;
        }

        context.Writer.WriteLine(expression + ";");
        return true;
    }
}
