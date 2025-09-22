using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class MovieCallVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count != 1 || tokens[0].Kind != BlSyntaxKind.IdentifierToken)
        {
            return false;
        }

        var handlerName = BlCSharpName.SanitizeIdentifier(tokens[0].ValueText);
        context.Writer.WriteLine($"CallMovieScript(movie => movie.{handlerName}());");
        return true;
    }
}
