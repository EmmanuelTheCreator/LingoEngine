using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Scans the token stream to discover declarations and populate the symbol table.
/// </summary>
public sealed class BlLingoDeclarationPass : BlLingoAnalysisPass
{
    public BlLingoDeclarationPass()
        : base("Declaration")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = context.Tokens;
        var symbols = context.Symbols;
        var commentDeclaredClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            DeclareClassFromCommentTrivia(token.LeadingTrivia, symbols, commentDeclaredClasses);
            if (token.Kind != BlSyntaxKind.KeywordToken)
            {
                continue;
            }

            var keyword = token.ValueText;
            if (keyword.Equals("global", StringComparison.OrdinalIgnoreCase))
            {
                index = CollectSeparatedIdentifiers(tokens, index + 1, symbols.DeclareGlobal);
            }
            else if (keyword.Equals("property", StringComparison.OrdinalIgnoreCase))
            {
                index = CollectSeparatedIdentifiers(tokens, index + 1, symbols.DeclareProperty);
            }
            else if (keyword.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                index = CollectHandler(tokens, index, symbols);
            }
            else if (keyword.Equals("script", StringComparison.OrdinalIgnoreCase))
            {
                index = CollectClassDeclaration(tokens, index + 1, symbols);
            }
            else if (keyword.Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                TryCloseHandler(tokens, index, symbols);
            }
        }
    }

    /// <summary>
    /// Collects a comma-separated list of identifiers until a newline boundary is encountered.
    /// </summary>
    private static int CollectSeparatedIdentifiers(
        IReadOnlyList<BlSyntaxToken> tokens,
        int startIndex,
        Func<BlSyntaxToken, BlCodeSymbol> declareSymbol)
    {
        var lastIndex = startIndex - 1;
        for (var index = startIndex; index < tokens.Count; index++)
        {
            var current = tokens[index];
            if (ContainsNewLine(current.LeadingTrivia))
            {
                break;
            }

            if (current.Kind == BlSyntaxKind.IdentifierToken)
            {
                declareSymbol(current);
                lastIndex = index;
                continue;
            }

            if (current.Kind == BlSyntaxKind.CommaToken)
            {
                lastIndex = index;
                continue;
            }

            break;
        }

        return lastIndex;
    }

    /// <summary>
    /// Discovers a handler declaration, records its name, and parses its parameter list.
    /// </summary>
    private static int CollectHandler(IReadOnlyList<BlSyntaxToken> tokens, int keywordIndex, BlLingoSymbolTable symbols)
    {
        var lastIndex = keywordIndex;
        var nameIndex = keywordIndex + 1;
        if (nameIndex >= tokens.Count)
        {
            return lastIndex;
        }

        var handlerToken = tokens[nameIndex];
        if (ContainsNewLine(handlerToken.LeadingTrivia))
        {
            return lastIndex;
        }

        if (!IsHandlerName(handlerToken))
        {
            return lastIndex;
        }

        symbols.EndHandler();
        var handler = symbols.BeginHandler(handlerToken);
        lastIndex = nameIndex;

        var sawParameter = false;
        var firstIsMe = false;

        for (var index = nameIndex + 1; index < tokens.Count; index++)
        {
            var current = tokens[index];
            if (ContainsNewLine(current.LeadingTrivia))
            {
                break;
            }

            if (IsParameterToken(current))
            {
                symbols.DeclareParameter(current);
                if (!sawParameter)
                {
                    sawParameter = true;
                    firstIsMe = string.Equals(current.ValueText, "me", StringComparison.OrdinalIgnoreCase);
                }
                lastIndex = index;
                continue;
            }

            if (current.Kind == BlSyntaxKind.CommaToken)
            {
                lastIndex = index;
                continue;
            }

            break;
        }

        handler.SetLeadingParameterInfo(sawParameter && firstIsMe);
        symbols.CurrentClass.ApplyScriptKind(handler.ImpliedScriptKind);

        return lastIndex;
    }

    private static bool IsParameterToken(BlSyntaxToken token)
    {
        if (token.Kind == BlSyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (token.Kind == BlSyntaxKind.KeywordToken)
        {
            return string.Equals(token.ValueText, "me", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// Parses a class declaration that follows a <c>script</c> keyword.
    /// </summary>
    private static int CollectClassDeclaration(IReadOnlyList<BlSyntaxToken> tokens, int startIndex, BlLingoSymbolTable symbols)
    {
        if (startIndex >= tokens.Count)
        {
            return startIndex - 1;
        }

        var token = tokens[startIndex];
        if (ContainsNewLine(token.LeadingTrivia))
        {
            return startIndex - 1;
        }

        if (token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.SymbolToken or BlSyntaxKind.StringLiteralToken)
        {
            var name = token.ValueText;
            if (!string.IsNullOrWhiteSpace(name))
            {
                symbols.DeclareClass(name, token);
                return startIndex;
            }
        }

        return startIndex - 1;
    }

    private static void DeclareClassFromCommentTrivia(
        IReadOnlyList<BlSyntaxTrivia> trivia,
        BlLingoSymbolTable symbols,
        HashSet<string> declaredClasses)
    {
        if (trivia is null || trivia.Count == 0)
        {
            return;
        }

        foreach (var item in trivia)
        {
            if (item.Kind != BlSyntaxKind.CommentTrivia)
            {
                continue;
            }

            var name = TryExtractClassName(item.ValueText);
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (declaredClasses.Add(name))
            {
                symbols.DeclareClass(name);
            }
        }
    }

    private static string? TryExtractClassName(string? commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return null;
        }

        const string Prefix = "script";
        if (!commentText.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var remainder = commentText[Prefix.Length..].TrimStart();
        if (string.IsNullOrEmpty(remainder))
        {
            return null;
        }

        if (remainder[0] == '"')
        {
            if (remainder.Length < 2)
            {
                return null;
            }

            var endQuote = remainder.IndexOf('"', 1);
            if (endQuote <= 1)
            {
                return null;
            }

            var quoted = remainder.Substring(1, endQuote - 1).Trim();
            return string.IsNullOrWhiteSpace(quoted) ? null : quoted;
        }

        var terminatorIndex = remainder.IndexOfAny(new[] { ' ', '\t' });
        var name = terminatorIndex >= 0 ? remainder[..terminatorIndex] : remainder;
        name = name.Trim();
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    /// <summary>
    /// Closes the current handler scope if the <c>end</c> keyword terminates it.
    /// </summary>
    private static void TryCloseHandler(IReadOnlyList<BlSyntaxToken> tokens, int index, BlLingoSymbolTable symbols)
    {
        if (symbols.CurrentHandler is null)
        {
            return;
        }

        if (IsHandlerTerminator(tokens, index))
        {
            symbols.EndHandler();
        }
    }

    /// <summary>
    /// Determines whether the supplied token can be used as a handler name.
    /// </summary>
    private static bool IsHandlerName(BlSyntaxToken token)
    {
        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken;
    }

    /// <summary>
    /// Determines whether an <c>end</c> token terminates the current handler scope.
    /// </summary>
    private static bool IsHandlerTerminator(IReadOnlyList<BlSyntaxToken> tokens, int index)
    {
        if (index >= tokens.Count)
        {
            return false;
        }

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
}
