using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Examines tokens to associate discovered symbols with type information and tracks unresolved entries.
/// </summary>
public sealed class BlLingoTypeLinkPass : BlLingoAnalysisPass
{
    public const string PendingTypeSymbolsKey = nameof(PendingTypeSymbolsKey);

    public BlLingoTypeLinkPass()
        : base("TypeLink")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var symbols = context.Symbols;
        var tokens = context.Tokens;

        var currentClass = symbols.MovieScript;
        BlLingoHandlerSymbolTable? currentHandler = null;

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];

            if (token.Kind == BlSyntaxKind.KeywordToken)
            {
                var keyword = token.ValueText;
                if (IsClassKeyword(keyword))
                {
                    if (TryGetClassName(tokens, index + 1, out var className) &&
                        symbols.ClassScopes.TryGetValue(className, out var scope))
                    {
                        currentClass = scope;
                        currentHandler = null;
                    }

                    continue;
                }

                if (keyword.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetHandlerNameToken(tokens, index + 1, out var handlerToken) &&
                        currentClass.Handlers.TryGetValue(handlerToken.ValueText, out var handlerScope))
                    {
                        currentHandler = handlerScope;
                    }
                    else
                    {
                        currentHandler = null;
                    }

                    continue;
                }

                if (keyword.Equals("end", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentHandler is not null && IsHandlerTerminator(tokens, index))
                    {
                        currentHandler = null;
                    }

                    continue;
                }
            }

            if (!IsPotentialAssignmentTarget(token))
            {
                continue;
            }

            if (TryReadScriptInstantiation(tokens, index, out var scriptName, out var consumedIndex))
            {
                var symbol = ResolveSymbol(symbols, currentClass, currentHandler, token.ValueText);
                if (symbol is not null)
                {
                    symbol.SetTypeCode(scriptName);
                    symbol.SetResolvedTypeName(scriptName);
                }

                index = consumedIndex;
            }
        }

        var pending = CollectPendingSymbols(symbols);
        context.SetData(PendingTypeSymbolsKey, pending);
    }

    private static List<BlCodeSymbol> CollectPendingSymbols(BlLingoSymbolTable symbols)
    {
        var pending = new List<BlCodeSymbol>();
        AddPending(symbols.Globals.Values, pending);
        AddPending(symbols.MovieScript.Properties.Values, pending);
        AddHandlerSymbols(symbols.MovieScript, pending);

        foreach (var classScope in symbols.ClassScopes.Values)
        {
            AddPending(classScope.Symbol, pending);
            AddPending(classScope.Properties.Values, pending);
            AddHandlerSymbols(classScope, pending);
        }

        return pending;
    }

    private static void AddPending(BlCodeSymbol symbol, ICollection<BlCodeSymbol> pending)
    {
        if (symbol.TypeCode is null || symbol.ResolvedTypeName is null)
        {
            pending.Add(symbol);
        }
    }

    private static void AddPending(IEnumerable<BlCodeSymbol> symbols, ICollection<BlCodeSymbol> pending)
    {
        foreach (var symbol in symbols)
        {
            AddPending(symbol, pending);
        }
    }

    private static void AddHandlerSymbols(BlLingoClassSymbolTable classScope, ICollection<BlCodeSymbol> pending)
    {
        foreach (var handler in classScope.Handlers.Values)
        {
            AddPending(handler.Symbol, pending);
            AddPending(handler.Parameters.Values, pending);
            AddPending(handler.Locals.Values, pending);
        }
    }

    private static bool IsClassKeyword(string keyword)
    {
        return keyword.Equals("script", StringComparison.OrdinalIgnoreCase) ||
               keyword.Equals("factory", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetClassName(IReadOnlyList<BlSyntaxToken> tokens, int index, out string name)
    {
        name = string.Empty;
        if (!TryGetToken(tokens, index, out var token))
        {
            return false;
        }

        if (ContainsNewLine(token.LeadingTrivia))
        {
            return false;
        }

        if (token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.SymbolToken or BlSyntaxKind.StringLiteralToken)
        {
            var value = token.ValueText;
            if (!string.IsNullOrWhiteSpace(value))
            {
                name = value;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetHandlerNameToken(
        IReadOnlyList<BlSyntaxToken> tokens,
        int index,
        out BlSyntaxToken handlerToken)
    {
        handlerToken = default!;
        if (!TryGetToken(tokens, index, out var token))
        {
            return false;
        }

        if (ContainsNewLine(token.LeadingTrivia))
        {
            return false;
        }

        if (token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken)
        {
            handlerToken = token;
            return true;
        }

        return false;
    }

    private static bool TryReadScriptInstantiation(
        IReadOnlyList<BlSyntaxToken> tokens,
        int identifierIndex,
        out string scriptName,
        out int consumedIndex)
    {
        scriptName = string.Empty;
        consumedIndex = identifierIndex;

        if (!TryGetToken(tokens, identifierIndex + 1, out var equalsToken) ||
            ContainsNewLine(equalsToken.LeadingTrivia) ||
            equalsToken.Kind != BlSyntaxKind.OperatorToken ||
            !string.Equals(equalsToken.ValueText, "=", StringComparison.Ordinal))
        {
            return false;
        }

        if (!TryGetToken(tokens, identifierIndex + 2, out var scriptKeyword) ||
            ContainsNewLine(scriptKeyword.LeadingTrivia) ||
            scriptKeyword.Kind != BlSyntaxKind.KeywordToken ||
            !scriptKeyword.ValueText.Equals("script", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!TryGetToken(tokens, identifierIndex + 3, out var openParen) ||
            ContainsNewLine(openParen.LeadingTrivia) ||
            openParen.Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        if (!TryGetToken(tokens, identifierIndex + 4, out var typeToken) ||
            ContainsNewLine(typeToken.LeadingTrivia))
        {
            return false;
        }

        var candidate = typeToken.Kind switch
        {
            BlSyntaxKind.StringLiteralToken => typeToken.ValueText,
            BlSyntaxKind.SymbolToken => typeToken.ValueText,
            BlSyntaxKind.IdentifierToken => typeToken.ValueText,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        if (!TryGetToken(tokens, identifierIndex + 5, out var closeParen) ||
            closeParen.Kind != BlSyntaxKind.RightParenthesisToken)
        {
            return false;
        }

        var currentIndex = identifierIndex + 5;
        if (TryGetToken(tokens, currentIndex + 1, out var periodToken) &&
            !ContainsNewLine(periodToken.LeadingTrivia) &&
            periodToken.Kind == BlSyntaxKind.PeriodToken)
        {
            currentIndex++;

            if (!TryGetToken(tokens, currentIndex + 1, out var newToken) ||
                ContainsNewLine(newToken.LeadingTrivia) ||
                newToken.Kind != BlSyntaxKind.IdentifierToken ||
                !newToken.ValueText.Equals("new", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            currentIndex++;

            if (TryGetToken(tokens, currentIndex + 1, out var invocationStart) &&
                !ContainsNewLine(invocationStart.LeadingTrivia) &&
                invocationStart.Kind == BlSyntaxKind.LeftParenthesisToken)
            {
                currentIndex++;
                var depth = 1;
                while (currentIndex + 1 < tokens.Count && depth > 0)
                {
                    var next = tokens[currentIndex + 1];
                    currentIndex++;
                    if (next.Kind == BlSyntaxKind.LeftParenthesisToken)
                    {
                        depth++;
                        continue;
                    }

                    if (next.Kind == BlSyntaxKind.RightParenthesisToken)
                    {
                        depth--;
                    }
                }

                if (depth != 0)
                {
                    return false;
                }
            }
        }

        scriptName = candidate!;
        consumedIndex = currentIndex;
        return true;
    }

    private static BlCodeSymbol? ResolveSymbol(
        BlLingoSymbolTable symbols,
        BlLingoClassSymbolTable currentClass,
        BlLingoHandlerSymbolTable? currentHandler,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (currentHandler is not null)
        {
            if (currentHandler.Locals.TryGetValue(name, out var local))
            {
                return local;
            }

            if (currentHandler.Parameters.TryGetValue(name, out var parameter))
            {
                return parameter;
            }
        }

        if (currentClass.Properties.TryGetValue(name, out var property))
        {
            return property;
        }

        if (!currentClass.IsMovieScript &&
            symbols.MovieScript.Properties.TryGetValue(name, out var movieProperty))
        {
            return movieProperty;
        }

        if (symbols.Globals.TryGetValue(name, out var global))
        {
            return global;
        }

        return null;
    }

    private static bool IsPotentialAssignmentTarget(BlSyntaxToken token)
    {
        if (token.Kind == BlSyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (token.Kind == BlSyntaxKind.KeywordToken)
        {
            return token.ValueText.Equals("me", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsHandlerTerminator(IReadOnlyList<BlSyntaxToken> tokens, int index)
    {
        var nextIndex = index + 1;
        while (nextIndex < tokens.Count)
        {
            var next = tokens[nextIndex];
            if (next.Kind == BlSyntaxKind.EndOfFileToken)
            {
                return true;
            }

            if (ContainsNewLine(next.LeadingTrivia))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private static bool ContainsNewLine(IReadOnlyList<BlSyntaxTrivia> trivia)
    {
        foreach (var item in trivia)
        {
            if (item.Kind == BlSyntaxKind.NewLineTrivia)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetToken(
        IReadOnlyList<BlSyntaxToken> tokens,
        int index,
        out BlSyntaxToken token)
    {
        if (index >= 0 && index < tokens.Count)
        {
            token = tokens[index];
            return true;
        }

        token = default!;
        return false;
    }
}
